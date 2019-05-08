using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLibrary
{
    class NNConversionCacheTester
    {
        public static void ClearCache()
        {
            SettingsData.Default.ClearNNConversions();
        }

        public static void PrintCache()
        {
            var cache = SettingsData.Default.ReadNNConversions();
            foreach (var conv in cache)
            {
                Log.Trace(conv.ToString());
            }
        }

        public static void RunCacheTests()
        {
            try
            {
                Log.Trace("RUNNING CACHE TESTS");
                TestCacheInsertAndUpdate();
                TestCacheHit();
                TestCacheMiss();
                TestCacheDeleteLRU();
                TestCacheOverload();
                CacheTestingClean();
                Log.Trace("FINISHED RUNNING CACHE TESTS");
            }
            catch (Exception e)
            {
                Log.Trace("RED FLAG - This error occurred while running cache tests");
                Log.Trace(e);
            }
        }

        private static void TestCacheInsertAndUpdate()
        {
            var no2 = 1.3345;
            var o3 = 2.3345;
            var co = -0.3345;
            var temp = 23.7;
            var absHum = 1.6678;
            var pres = 1001.2;
            var no2PPB = 12.5;
            var o3PPB = 40.33;
            var coPPM = 4.2;
            var beforeInsertCount = SettingsData.Default.ReadNNConversions().Count();

            Log.Trace($"Inserting a conversion with {no2}  {o3}  {co}  {temp}  {absHum}  {pres}" +
                $"{no2PPB}  {o3PPB}  {coPPM}");
            var rowsAdded = SettingsData.Default.AddNNConversion(no2, o3, co, temp, absHum, pres,
                no2PPB, o3PPB, coPPM);

            if (beforeInsertCount + 1 == SettingsData.Default.ReadNNConversions().Count() ||
                beforeInsertCount == SettingsData.NN_CONVERSION_CACHE_SIZE &&
                SettingsData.Default.ReadNNConversions().Count() == SettingsData.NN_CONVERSION_CACHE_SIZE)
            {
                Log.Trace("~~~~PASSED CACHE INSERT~~~~");
            }
            else
            {
                Log.Trace("----FAILED CACHE INSERT----");
                Log.Trace($"Before insert - count: {beforeInsertCount}");
                Log.Trace($"Expected | After insert - count: {beforeInsertCount + 1}");
                Log.Trace($"Actual | After insert - count: {SettingsData.Default.ReadNNConversions().Count()}");
                Log.Trace("Expected | 1 row added");
                Log.Trace($"Actual | {rowsAdded} row added");
            }

            var initialConv = SettingsData.Default.Get(no2, o3, co, temp, absHum, pres);
            var initialConvTs = initialConv.Ts;
            var initialTick = SettingsData.Default.GetCountValue("NNConversionCacheClock");

            SettingsData.Default.UpdateNNConversionTime(no2, o3, co, temp, absHum, pres);
            Log.Trace("Updating the same conversion again");

            var databaseConv = SettingsData.Default.Get(no2, o3, co, temp, absHum, pres);
            if (initialConvTs + 3 == databaseConv.Ts)
            {
                Log.Trace("~~~~PASSED CACHE UPDATE~~~~");
            }
            else
            {
                Log.Trace("----FAILED CACHE UPDATE----");
                Log.Trace($"Expected | Time: {initialConvTs + 2}");
                Log.Trace($"Actual | Time: {databaseConv.Ts}");
            }
        }

        private static void TestCacheHit()
        {
            var no2 = 1.3345;
            var o3 = 2.3345;
            var co = -0.3345;
            var temp = 23.7;
            var absHum = 1.6678;
            var pres = 1001.2;
            var no2PPB = 12.5;
            var o3PPB = 40.33;
            var coPPM = 4.2;

            var cacheHit = SettingsData.Default.GetNNConversionResults(no2, o3, co, temp, absHum, pres);
            Log.Trace($"Searching for conversion with features: {no2}  {o3}  {co}  {temp}  " +
                $"{absHum}  {pres}");
            if (cacheHit != null)
            {
                Log.Trace("~~~~PASSED CACHE HIT~~~~");
            }
            else
            {
                Log.Trace("----FAILED CACHE HIT----");
                Log.Trace($"Expected | Conversions: {no2PPB}  {o3PPB}  {coPPM}");
                Log.Trace($"Actual | Conversions: {cacheHit[0]}  {cacheHit[1]}  {cacheHit[2]}");
            }

            var testConversion = SettingsData.Default.ReadNNConversions().Where(c => c.COppm > 0).First();
            Log.Trace($"Searching for conversion with features: {testConversion.NO2}  " +
                $"{testConversion.O3}  {testConversion.CO}  {testConversion.Temperature}  " +
                $"{testConversion.AbsoluteHumidity}  {testConversion.Pressure}");

            var storedConversions = SettingsData.Default.Get(testConversion.NO2,
                testConversion.O3, testConversion.CO, testConversion.Temperature,
                testConversion.AbsoluteHumidity, testConversion.Pressure);
            if (cacheHit != null)
            {
                Log.Trace($"~~~~PASSED CACHE HIT~~~~");
            }
            else
            {
                Log.Trace($"----FAILED CACHE HIT----");
                Log.Trace($"Conversions: {storedConversions.NO2ppb}  " +
                    $"{storedConversions.O3ppb}  {storedConversions.COppm}");
            }
        }

        private static void TestCacheMiss()
        {
            // pres needs to be 1001.2 for cache hit
            var no2 = 1.3345;
            var o3 = 2.3345;
            var co = -0.3345;
            var temp = 23.7;
            var absHum = 1.6678;
            var pres = 1001;

            var cacheMiss = SettingsData.Default.GetNNConversionResults(no2, o3, co, temp, absHum, pres);
            Log.Trace($"Searching for conversion with features: {no2}  {o3}  {co}  {temp}  " +
                $"{absHum}  {pres}");
            if (cacheMiss == null)
            {
                Log.Trace($"~~~~PASSED CACHE MISS~~~~");
            }
            else
            {
                Log.Trace($"----FAILED CACHE MISS----");
                Log.Trace($"Conversions for wrong cache hit: {cacheMiss[0]}  {cacheMiss[1]}  {cacheMiss[2]}");
            }
        }

        private static void TestCacheDeleteLRU()
        {
            var initialCount = SettingsData.Default.ReadNNConversions().Count();
            var leastRecentlyUsed = SettingsData.Default.ReadNNConversions().OrderBy(c => c.Ts).First();
            Log.Trace("Deleting: " + leastRecentlyUsed.ToString());
            SettingsData.Default.DeleteNNConversionsLRU(1);

            Log.Trace("Searching for: " + leastRecentlyUsed.ToString());
            var cacheMiss = SettingsData.Default.Get(leastRecentlyUsed.NO2, leastRecentlyUsed.O3,
                leastRecentlyUsed.CO, leastRecentlyUsed.Temperature,
                leastRecentlyUsed.AbsoluteHumidity, leastRecentlyUsed.Pressure);
            if (cacheMiss == null)
            {
                Log.Trace("~~~~PASSED DELETE LRU~~~~");
            }
            else
            {
                Log.Trace($"----FAILED DELETE LRU----");
            }

            Log.Trace($"Adding this deleted conversion back in: {leastRecentlyUsed.ToString()}");
            SettingsData.Default.AddNNConversion(leastRecentlyUsed.NO2, leastRecentlyUsed.O3,
                leastRecentlyUsed.CO, leastRecentlyUsed.Temperature,
                leastRecentlyUsed.AbsoluteHumidity, leastRecentlyUsed.Pressure,
                leastRecentlyUsed.NO2ppb, leastRecentlyUsed.O3ppb, leastRecentlyUsed.COppm);
            Log.Trace("Searching for: " + leastRecentlyUsed.ToString());
            var cacheHit = SettingsData.Default.Get(leastRecentlyUsed.NO2, leastRecentlyUsed.O3,
                leastRecentlyUsed.CO, leastRecentlyUsed.Temperature,
                leastRecentlyUsed.AbsoluteHumidity, leastRecentlyUsed.Pressure);
            if (cacheHit != null)
            {
                Log.Trace($"~~~~REINSERT SUCCESS~~~~");
            }
            else
            {
                Log.Trace($"----REINSERT FAILURE----");
            }
        }

        private static void TestCacheOverload()
        {
            Log.Trace("Testing multiple inserts that would cause cache to exceed capacity");
            Log.Trace("UNCOMMENT TO TEST AND MANUALLY CHECK");

            var i = 1;
            while (i < SettingsData.NN_CONVERSION_CACHE_SIZE * 2)
            {
                SettingsData.Default.AddNNConversion(i, 0, 0, 0, 0, 0, 0, 0, 0);
                /* Manual check
                Log.Trace($"Inserted conversion with features: {i}  0  0  0  0  0  0  0  0");
                Log.Trace("#####################################");
                PrintCache();
                Log.Trace("#####################################");
                */
                i++;
            }

            ClearCache();

            Log.Trace("Testing multiple inserts and updates that would cause cache to exceed capacity");
            Log.Trace("UNCOMMENT TO TEST AND MANUALLY CHECK");

            var j = 1;
            var k = 1;
            while (j < SettingsData.NN_CONVERSION_CACHE_SIZE * 2)
            {
                SettingsData.Default.AddNNConversion(j, 0, 0, 0, 0, 0, 0, 0, 0);
                /* Manual check
                Log.Trace($"Inserted conversion with features: {j}  0  0  0  0  0  0  0  0");
                Log.Trace("#####################################");
                PrintCache();
                Log.Trace("#####################################");
                */
                if (j % 3 == 0)
                {
                    SettingsData.Default.UpdateNNConversionTime(k, 0, 0, 0, 0, 0);
                    /* Manual check
                    Log.Trace($"Updated conversion with features: {k}  0  0  0  0  0  0  0  0");
                    Log.Trace("#####################################");
                    PrintCache();
                    Log.Trace("#####################################");
                    */
                    k += 3;
                }
                j++;
            }
        }

        private static void CacheTestingClean()
        {
            ClearCache();

            if (SettingsData.Default.ReadNNConversions().Count() == 0)
            {
                Log.Trace("~~~~CLEAN SUCCESS~~~~");
            }
            else
            {
                Log.Trace("----CLEAN FAILURE----");
            }
        }
    }
}
