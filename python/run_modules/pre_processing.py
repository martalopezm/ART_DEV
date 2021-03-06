import subprocess
import common.file_modifier as file_modifier
import common.config as cfg
import os
import common.constants as static



#modifies START and END parameter on .dat files.
def dat_date_change(filePath):
  file_modifier.modify_line(filePath, "START", file_modifier.date_to_mohid_date(cfg.current_initial_date))
  file_modifier.modify_line(filePath, "END", file_modifier.date_to_mohid_date(cfg.current_final_date))


'''
Function to run pre processing blocks. Exe path can be the path to a executable or a shell comand.
If the exe or command need any flags or arguments it can be given in flags parameter in the yaml file.
'''
def execute(yaml):
  #each block has a unique name
  for block in yaml['preProcessing']:
    block_keys = yaml['preProcessing'][block].keys()
    block_dict = yaml['preProcessing'][block]
    if block_dict['run']:
      
      #Script config file needs to change START and END to the run time
      if 'datDateChange' in  block_keys and block_dict['datDateChange']:
        dat_date_change(block_dict['configFilePath'])
      #if the command/executable needs arguments/flags the argument for subprocess.run needs to be a list with every
      #argument separeted. example ['ls', 'l']. 
      
      if 'flags' in block_keys:
        #flags are given in a single string, we must create a list with each argument/flag separated
        flags_array = block_dict['flags'].split(" ") 
        run_array = [block_dict['exePath']] + flags_array

        if 'outputToFile' in block_keys and block_dict['outputToFile']:
          with open(block_dict['outputFilePath'], 'w') as log:
            subprocess.run(run_array, stdout=log, cwd=block_dict['workingDirectory'])
            log.close()
        else:
          subprocess.run(run_array, cwd=block_dict['workingDirectory'])
      
      #First argument of subprocess.run can be just the command/path to executable as there is no need for a list.
      else:
        if 'outputToFile' in block_keys and block_dict['outputToFile']:
         with open(block_dict['outputFilePath'], 'w') as log:
            static.logger.info("Executing Pre Processing module: " +  block_dict['exePath'])
            subprocess.run(block_dict['exePath'], stdout=log, cwd=block_dict['workingDirectory'])
            log.close()
        else:
          static.logger.info("Executing Pre Processing module: " +  block_dict['exePath'])
          subprocess.run(block_dict['exePath'], cwd=block_dict['workingDirectory'])

  return  