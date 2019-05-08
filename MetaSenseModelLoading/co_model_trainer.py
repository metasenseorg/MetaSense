from metasense.data import load
from metasense.models import *
from sklearn.metrics import mean_squared_error
from deepx import nn
import math
import numpy as np
import json
import joblib

PICKLE_PATH = '../../MyModels/NN4/alt_board18_co.pkl'

def main():
  """
  Train a CO neural network model and save as a pickle file.

  Note: must add this line to metasense-transfer/metasense/data.py on line 9
  data['epa-co'] = data['co']
  """

  X_features = ['no2', 'o3', 'co', 'temperature', 'absolute-humidity', \
								'pressure']
  Y_features = ['epa-co']

  nn_model = NeuralNetwork(X_features, nn.Relu(len(X_features[:]), 200) >> \
      nn.Relu(200) >> nn.Relu(200) >> nn.Relu(200) >> nn.Linear(1)) 

  round_num = 3
  location = 'elcajon'
  board = 18

  train, test = load(round_num, location, board)
  nn_model.fit(train[X_features], train[Y_features])

  joblib.dump((board, nn_model), PICKLE_PATH)
  print(f'Saved pickle of model to {PICKLE_PATH}')

  """ print predictions
  predictions = nn_model.predict(test[X_features])
  print('----Predictions on test set----')
  print(predictions)
  print('----EPA data for test set----')
  print(test[Y_features])
  """

  """ print testing error
  print('----Testing error----')
  mse = mean_squared_error(test[Y_features].values, predictions)
  print(f'Mean squared error: {mse}')
  print(f'Mean error: {math.sqrt(mse)}')
  """

if __name__ == '__main__':
  main()
