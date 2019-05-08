using System;
using System.Collections.Generic;

namespace BackendAPI.Compression
{
    enum HSD_state
    {
        HSDS_TAG_BIT,               /* tag bit */
        HSDS_YIELD_LITERAL,         /* ready to yield literal byte */
        HSDS_BACKREF_INDEX_MSB,     /* most significant byte of index */
        HSDS_BACKREF_INDEX_LSB,     /* least significant byte of index */
        HSDS_BACKREF_COUNT_MSB,     /* most significant byte of count */
        HSDS_BACKREF_COUNT_LSB,     /* least significant byte of count */
        HSDS_YIELD_BACKREF,         /* ready to yield back-reference */
    }

    enum HSD_sink_res
    {
        HSDR_SINK_OK,               /* data sunk, ready to poll */
        HSDR_SINK_FULL,             /* out of space in internal buffer */
        HSDR_SINK_ERROR_NULL = -1,    /* NULL argument */
    }

    enum HSD_poll_res
    {
        HSDR_POLL_EMPTY,            /* input exhausted */
        HSDR_POLL_MORE,             /* more data remaining, call again w/ fresh output buffer */
        HSDR_POLL_ERROR_NULL = -1,    /* NULL arguments */
        HSDR_POLL_ERROR_UNKNOWN = -2,
    }

    enum HSD_finish_res
    {
        HSDR_FINISH_DONE,           /* output is done */
        HSDR_FINISH_MORE,           /* more output remains */
        HSDR_FINISH_ERROR_NULL = -1,  /* NULL arguments */
    }


    //#define HEATSHRINK_STATIC_INPUT_BUFFER_SIZE 32
    //#define HEATSHRINK_STATIC_WINDOW_BITS 8
    //#define HEATSHRINK_STATIC_LOOKAHEAD_BITS 4
    //#define HEATSHRINK_DECODER_LOOKAHEAD_BITS(BUF) \
    //    (HEATSHRINK_STATIC_LOOKAHEAD_BITS)

    public class HeatshrinkDecoder
    {
        private const int HEATSHRINK_STATIC_INPUT_BUFFER_SIZE = 32;
        private const int HEATSHRINK_STATIC_WINDOW_BITS = 11;
        private const int HEATSHRINK_STATIC_LOOKAHEAD_BITS = 4;

        /* Version 0.4.1 */
        private const int HEATSHRINK_VERSION_MAJOR = 0;
        private const int HEATSHRINK_VERSION_MINOR = 4;
        private const int HEATSHRINK_VERSION_PATCH = 1;

        private const int HEATSHRINK_MIN_WINDOW_BITS = 4;
        private const int HEATSHRINK_MAX_WINDOW_BITS = 15;

        private const int HEATSHRINK_MIN_LOOKAHEAD_BITS = 3;

        private const int HEATSHRINK_LITERAL_MARKER = 0x01;
        private const int HEATSHRINK_BACKREF_MARKER = 0x00;

        //private const int BACKREF_COUNT_BITS(HSD) (HEATSHRINK_DECODER_LOOKAHEAD_BITS(HSD))
        //private const int BACKREF_INDEX_BITS(HSD) (HEATSHRINK_DECODER_WINDOW_BITS(HSD))

            
        byte[] buf;               /* output buffer */
        int buf_size;            /* buffer size */
        int output_size;

        int input_size;        /* bytes in input buffer */
        UInt16 input_index;       /* offset to next unprocessed input byte */
        int output_count;      /* how many bytes to output */
        UInt16 output_index;      /* index for bytes to output */
        UInt16 head_index;        /* head of window buffer */
        HSD_state state;              /* current state machine node */
        byte current_byte;       /* current byte of input */
        byte bit_index;          /* current bit index */

        private int buf_sz;
        private int input_sz;

        /* Input buffer, then expansion window buffer */
        byte[] buffers = new byte[(1 << HEATSHRINK_STATIC_WINDOW_BITS) + HEATSHRINK_STATIC_INPUT_BUFFER_SIZE];

        private int _inPos;
        private byte[] _inBuffer;
        private List<byte> _outBuffer;
        private byte[] _decodeWindow;
        private int _decWinPos;

        private int _inSize => _inBuffer.Length * 8 - _inPos;
        
        public HeatshrinkDecoder(byte[] inBuffer)
        {
            _inPos = 0;
            _decWinPos = 0;
            _inBuffer = inBuffer;
            _outBuffer = new List<byte>(_inBuffer.Length);
            _decodeWindow = new byte[decodeWindowSize];
            heatshrink_decoder_reset();
        }
        private void heatshrink_decoder_reset()
        {
            buf_sz = (1 << HEATSHRINK_STATIC_WINDOW_BITS);
            input_sz = HEATSHRINK_STATIC_INPUT_BUFFER_SIZE;
            Array.Clear(buffers, 0, buf_sz + input_sz);
            state = HSD_state.HSDS_TAG_BIT;
            input_size = 0;
            input_index = 0;
            bit_index = 0x00;
            current_byte = 0x00;
            output_count = 0;
            output_index = 0;
            head_index = 0;
        }



        UInt16 get_bits(byte count)
        {
            int accumulator = 0;
            if (count > 15 || _inSize < count) { throw new Exception("No Bits"); }

            int start_byte = _inPos / 8;
            int end_byte = (_inPos + count) / 8;
            
            for (int i = 0; i <count; i++)
            {
                int current_byte = (_inPos + i) / 8;
                int byte_offset_mask = (1 << (7 - (_inPos + i) % 8));
                accumulator = (accumulator << 1);
                if ((_inBuffer[current_byte] & byte_offset_mask) > 0)
                    accumulator += 1;
            }
            _inPos += count;
            return (UInt16)accumulator;
        }

        HSD_sink_res heatshrink_decoder_sink(byte[] in_buf, int size, ref int input_size)
        {
            if (in_buf == null)
                return HSD_sink_res.HSDR_SINK_ERROR_NULL;

            int rem = input_sz - input_size;

            if (rem == 0)
            {
                input_size = 0;
                return HSD_sink_res.HSDR_SINK_FULL;
            }
            
            size = rem < size ? rem : size;

            //LOG("-- sinking %zd bytes\n", size);

            /* copy into input buffer (at head of buffers) */

            Array.Copy(buffers, input_size, in_buf, 0, size);
            this.input_size += size;
            input_size = size;
            return HSD_sink_res.HSDR_SINK_OK;
        }
        

        HSD_state st_tag_bit()
        {
            try
            {
                UInt16 bits = get_bits(1);
                if (bits != 0)
                {
                    return HSD_state.HSDS_YIELD_LITERAL;
                }
                return HSD_state.HSDS_BACKREF_INDEX_MSB;
            }
            catch (Exception) { return HSD_state.HSDS_TAG_BIT; }
        }

        private UInt16 bitMask => (1 << HEATSHRINK_STATIC_WINDOW_BITS) - 1;
        private UInt16 decodeWindowSize => (1 << HEATSHRINK_STATIC_WINDOW_BITS);

        HSD_state st_yield_literal()
        {
            /* Emit a repeated section from the window buffer, and add it (again)
             * to the window buffer. (Note that the repetition can include
             * itself.)*/
            try
            {
                UInt16 litBits = get_bits(8);
                byte c = (byte)(litBits & 0xFF);
                _outBuffer.Add(c);
                _decodeWindow[_decWinPos & bitMask] = c;
                _decWinPos++;
                return HSD_state.HSDS_TAG_BIT;
            }
            catch (Exception) { return HSD_state.HSDS_YIELD_LITERAL; }
        }
        
        HSD_state st_backref_index_msb()
        {
            byte bit_ct = HEATSHRINK_STATIC_WINDOW_BITS;
            //ASSERT(bit_ct > 8);
            try
            {
                UInt16 bits = get_bits((byte)(bit_ct - 8));

                output_index = (UInt16) (bits << 8);
                return HSD_state.HSDS_BACKREF_INDEX_LSB;
            }
            catch (Exception) { return HSD_state.HSDS_BACKREF_INDEX_MSB; }
        }

        HSD_state st_backref_index_lsb()
        {
            byte bit_ct = HEATSHRINK_STATIC_WINDOW_BITS;
            try
            {
                UInt16 bits = get_bits((byte)(bit_ct < 8 ? bit_ct : 8));

                output_index |= bits;
                output_index++;
                byte br_bit_ct = HEATSHRINK_STATIC_LOOKAHEAD_BITS;
                output_count = 0;
                return (br_bit_ct > 8) ? HSD_state.HSDS_BACKREF_COUNT_MSB : HSD_state.HSDS_BACKREF_COUNT_LSB;
            }
            catch (Exception) { return HSD_state.HSDS_BACKREF_INDEX_LSB; }
        }

        HSD_state st_backref_count_msb()
        {
            byte br_bit_ct = HEATSHRINK_STATIC_LOOKAHEAD_BITS;
            //ASSERT(br_bit_ct > 8);
            try
            {
                UInt16 bits = get_bits((byte)(br_bit_ct - 8));
                output_count = (UInt16)(bits << 8);
                return HSD_state.HSDS_BACKREF_COUNT_LSB;
            }
            catch (Exception) { return HSD_state.HSDS_BACKREF_COUNT_MSB; }
            //LOG("-- backref count (msb), got 0x%04x (+1)\n", bits);
        }

        HSD_state st_backref_count_lsb()
        {
            byte br_bit_ct = HEATSHRINK_STATIC_LOOKAHEAD_BITS;
            try
            {
                UInt16 bits = get_bits(br_bit_ct < 8 ? br_bit_ct : (byte)8);
                output_count |= bits;
                output_count++;
                return HSD_state.HSDS_YIELD_BACKREF;
            }
            catch (Exception) { return HSD_state.HSDS_BACKREF_COUNT_LSB; }
            //LOG("-- backref count (lsb), got 0x%04x (+1)\n", bits);
        }

        HSD_state st_yield_backref()
        {
            for (int i = 0; i < output_count; i++)
            {
                byte c = _decodeWindow[(_decWinPos - output_index) & bitMask];
                _decodeWindow[_decWinPos & bitMask] = c;
                _decWinPos++;
                _outBuffer.Add(c);
            }
            return HSD_state.HSDS_TAG_BIT; 
        }

        /*****************

         * Decompression *

         *****************/

        public byte[] Decode()
        {
            while (true)
            {
                HSD_state in_state = state;
                switch (in_state)
                {
                    case HSD_state.HSDS_TAG_BIT:
                        state = st_tag_bit();
                        break;
                    case HSD_state.HSDS_YIELD_LITERAL:
                        state = st_yield_literal();
                        break;
                    case HSD_state.HSDS_BACKREF_INDEX_MSB:
                        state = st_backref_index_msb();
                        break;
                    case HSD_state.HSDS_BACKREF_INDEX_LSB:
                        state = st_backref_index_lsb();
                        break;
                    case HSD_state.HSDS_BACKREF_COUNT_MSB:
                        state = st_backref_count_msb();
                        break;
                    case HSD_state.HSDS_BACKREF_COUNT_LSB:
                        state = st_backref_count_lsb();
                        break;
                    case HSD_state.HSDS_YIELD_BACKREF:
                        state = st_yield_backref();
                        break;
                    default:
                        throw new Exception("HSD_poll_res.HSDR_POLL_ERROR_UNKNOWN");
                }
                if (in_state == state)
                {
                    return _outBuffer.ToArray();
                }
            }
        }
    }
}
