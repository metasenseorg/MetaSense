import joblib
import numpy as np
import pandas as pd

READS_FROM_LOG_CSV = \
    '/mnt/d/Internships/Jenni_Deployment/Board12_ReadsFromLog.csv'
BOARD_MODEL_PICKLE = '/mnt/d/Internships/SharadModels/board12.pkl'
JUNE_18_2018_START = 1529280000
CELSIUS_TO_KELVIN_CONSTANT = 273.15
NO2_PPB_INDEX = 0
O3_PPB_INDEX = 1
SAVE_CSV_PATH = '/mnt/d/Internships/Jenni_Deployment/Board12_ReadsWithPPBs.csv'

def calculate_abs_humidity(rel_humidity, T):
  """!
  Calculate the absolute humidity.

  @param rel_humidity Float: relative humidity percent
  @param T Float: temperature (kelvin)

  @return Return a Float representing the absolute humidity.
  """
  return rel_humidity / 100 * np.exp(54.842763 - 6763.22 / T - 4.210 * \
      np.log(T) + 0.000367 * T + np.tanh(0.0415 * (T - 218.8)) * \
      (53.878 - 1331.22 / T - 9.44523 * np.log(T) + 0.014025 * T)) / 1000 

def date_search(df, unix_ts_date):
  """!
  Create a filtered DataFrame that only contains entries that occurred on or
  past the specified date.

  @param df DataFrame: contains the readings from the sensor
  @param unix_ts_date Integer: the universal unix time of the starting date from
                               which to examine the data

  @return Return a Dataframe that only contains entries that occurred on or
          past the specified date.
  """

  filtered_df = df.loc[df['Ts'] >= unix_ts_date]
  return filtered_df

def save_predictions(df, path):
  """!
  Save the DataFrame as a CSV file.

  @param df DataFrame: the DataFrame to save
  @param path String: the path of the file to save as
  """
  df.to_csv(path, index=False)

def main():
  """
  Predict NO2 and O3 from the sensor's readings using a neural network model.
  """
  reads_df = pd.read_csv(READS_FROM_LOG_CSV)
  info_df = date_search(reads_df, JUNE_18_2018_START).copy(deep=False)
  final_df = info_df.copy(deep=True)

  test = True
  if (test):
    info_df = info_df.head()
    final_df = final_df.head()

  # add columns with necessary information for prediction 
  for index, row in info_df.iterrows():
    info_df.loc[index, 'no2A-W'] = row['No2AmV'] - row['No2WmV']
    info_df.loc[index, 'o3A-W'] = row['OxAmV'] - row['OxWmV']
    info_df.loc[index, 'coA-W'] = row['CoAmV'] - row['CoWmV']
    info_df.loc[index, 'abs_hum'] = calculate_abs_humidity( \
        row['Hum_pc'], row['Hum_cT'] + CELSIUS_TO_KELVIN_CONSTANT)

  # create a dataframe only containing the features needed for prediction
  relevant_info_df = pd.DataFrame([info_df['no2A-W'], \
      info_df['o3A-W'], info_df['coA-W'], \
      info_df['Hum_cT'], info_df['abs_hum'], \
      info_df['Pres_mB']])
  relevant_info_df = relevant_info_df.T

  # predict no2(ppb) and o3(ppb) using the model
  board_num, model = joblib.load(BOARD_MODEL_PICKLE)
  predictions = model.predict(relevant_info_df.values)

  # update the dataframe (that was filtered by date) with the ppb values
  pred_index = 0
  for index, row in final_df.iterrows():
    final_df.loc[index, 'No2_ppb'] = predictions[pred_index][NO2_PPB_INDEX]
    final_df.loc[index, 'O3_ppb'] = predictions[pred_index][O3_PPB_INDEX]
    pred_index += 1

  if (not test):
    save_predictions(final_df, SAVE_CSV_PATH)
  else:
    print(final_df)

if __name__ == '__main__':
  main()
