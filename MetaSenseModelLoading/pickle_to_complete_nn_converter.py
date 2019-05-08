import joblib
import jsonpickle
import pickle
import json
import numpy as np

NN4_NO2_O3_PICKLE = \
    '/mnt/d/Internships/SharadCompleteModels/NeuralNetwork4Models/board18.pkl'
MULTI_SENSOR_NO2_O3_PICKLE = \
    '/mnt/d/Internships/SharadCompleteModels/MultiSensorModels/' + \
    'linear_relu2-100/3-elcajon-20.pkl'
NN4_CO_PICKLE = '/mnt/d/Internships/MyModels/NN4/alt_board18_co.pkl'

BASE_JSON_FILE_PATH = '../ModelJSONs/'
NN_TYPE_DIR = 'MultiSensor/'
BOARD_NUM = 18
JSON_FILE_PATH = BASE_JSON_FILE_PATH + NN_TYPE_DIR + 'alt_complete_board' + \
                 str(BOARD_NUM) + '_from_pickles.json'

CO_SPEC_KEY = 'co'
NO2_O3_SPEC_KEY = 'no2_o3'

def main():
  """Save the parameters of multiple models to one JSON file.""" 
  # add the activation functions, weights, and biases to the model dictionary
  model_dict = {}
  add_split_model_params(model_dict, MULTI_SENSOR_NO2_O3_PICKLE)
  add_neural_net_params(model_dict, NN4_CO_PICKLE, True)
  
  """ used for complete NN4 model
  add_params(model_dict, NN4_NO2_O3_PICKLE, False)
  add_params(model_dict, NN4_CO_PICKLE, True) 
  """

  # store the model dictionary as a json file
  json_file = JSON_FILE_PATH
  with open(json_file, 'w') as jfile:
    json.dump(model_dict, jfile)
  print(f'Saved as {json_file}')

def add_neural_net_params(m_dict, pickle_file, is_co_model):
  """!
  Add the parameters of a simple neural network to the specified dictionary.

  @param m_dict Dictionary: the dictionary to add parameters to
  @param pickle_file String: the path to the pickle of the model
  @param is_co_model Boolean: whether the pickle file is of a model that
                              predicts CO or not (NO2 and O3)
  """
  act_functions_key = 'act_functions_'
  weights_key = 'weights_'
  biases_key = 'biases_'
  board_num, model = joblib.load(pickle_file)
  model_info = model.model
  
  # modify dictionary keys to correspond to the model
  if is_co_model:
    act_functions_key += CO_SPEC_KEY
    weights_key += CO_SPEC_KEY
    biases_key += CO_SPEC_KEY
  else:
    act_functions_key += NO2_O3_SPEC_KEY
    weights_key += NO2_O3_SPEC_KEY
    biases_key += NO2_O3_SPEC_KEY
  
  # parse activation functions for each layer 
  layer_info = str(model_info).split(' >> ')
  for i in range(0, len(layer_info)):
    layer_info[i] = ''.join(char for char in layer_info[i] if char.isalpha())
    layer_info[i] = layer_info[i].lower()
  m_dict[act_functions_key] = layer_info

  model_parameters = model.model.get_parameters()
  weights_biases = model.session.run(model_parameters)
  weights = []
  biases = []

  # store the weights and biases under the appropriate key in the dictionary
  for (param, matrix) in zip(model_parameters, weights_biases):
    param_key = param.name.lower()
    if 'w' in param_key:
      weights.append(matrix.tolist())
    elif 'b' in param_key:
      biases.append(matrix.tolist())

  m_dict[weights_key] = weights
  m_dict[biases_key] = biases

def add_split_model_params(m_dict, split_model_pickle):
  """
  Add the sensor and calibration neural network parameters of the split model
  to the specified model dictionary.

  @param m_dict Dictionary: the dictionary to add parameters to
  @param split_model_pickle String: the path to the pickle of the split model
  """
  split_model = joblib.load(split_model_pickle)
  add_split_model_params_spec(m_dict, split_model, 'sensor_no2_o3', True,
      BOARD_NUM)
  add_split_model_params_spec(m_dict, split_model, 'calibration_no2_o3', False,
      BOARD_NUM)
  

def add_split_model_params_spec(m_dict, split_model, uniq_key,
                                want_sensor_model, board):
  """
  Add the sensor or calibration neural network parameters of the split model
  to the specified model dictionary.

  @param m_dict Dictionary: the dictionary to add parameters to
  @param split_model SplitModel: the split model possessing the parameters
  @param uniq_key String: part of the distinguishing key to be used in the 
                          dictionary
  @param want_sensor_model Boolean: whether the parameters of the sensor neural
                                    network should be added or not (calibration
                                    neural network instead)
  @param board Integer: the board number that corresponds to the desired sensor
                        neural network
  """
  act_functions_key = 'act_functions_' + uniq_key
  weights_key = 'weights_' + uniq_key
  biases_key = 'biases_' + uniq_key

  # set model characteristics to the sensor or calibration model
  sensor_weights, calibration_weights = split_model.get_weights()

  if want_sensor_model:
    model = split_model.sensor_map[board]
    weights_biases = sensor_weights[board]
  else:
    model = split_model.calibration_model
    weights_biases = calibration_weights

  model_info = str(model)

  # parse activation functions for each layer 
  layer_info = str(model_info).split(' >> ')
  for i in range(0, len(layer_info)):
    layer_info[i] = ''.join(char for char in layer_info[i] if char.isalpha())
    layer_info[i] = layer_info[i].lower()
  m_dict[act_functions_key] = layer_info

  model_parameters = model.get_parameters()
  weights = []
  biases = []

  # store the weights and biases under the appropriate key in the dictionary
  for (param, matrix) in zip(model_parameters, weights_biases):
    param_key = param.name.lower()
    if 'w' in param_key:
      weights.append(matrix.tolist())
    elif 'b' in param_key:
      biases.append(matrix.tolist())

  m_dict[weights_key] = weights
  m_dict[biases_key] = biases

if __name__ == '__main__':
  main()
