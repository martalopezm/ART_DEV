from lxml import etree

root = etree.Element('ART')
config = etree.SubElement(root, "config")
artconfig = etree.SubElement(config, "artconfig")
mainPath = etree.SubElement(artconfig, "mainPath")
mainPath.text = "TEXT"
forecastMode = etree.SubElement(artconfig, "forecastMode")
forecastMode.text = "0"
daysPerRun = etree.SubElement(artconfig, "daysPerRun")
daysPerRun.text = "0"
refDayToStart = etree.SubElement(artconfig, "refDayToStart")
refDayToStart.text = "0"
numberOfRuns = etree.SubElement(artconfig, "numberOfRuns")
numberOfRuns.text = "0"
runPreProcessing = etree.SubElement(artconfig, "runPreProcessing")
runPreProcessing.text = "0"
runMohid = etree.SubElement(artconfig, "runMohid")
runMohid.text = "0"
runPostProcessing = etree.SubElement(artconfig, "runPostProcessing")
runPostProcessing.text = "0"
start = etree.SubElement(artconfig, "start")
start.text = "data"
end = etree.SubElement(artconfig, "end")
end.text = "data"


#MOHID CONFIGURATION
mohid = etree.SubElement(config, "mohid")
mpi = etree.SubElement(mohid, "mpi") 
mpiEnable = etree.SubElement(mpi, "mpiEnable")
mpiEnable.text = "0"
mpiNumDomains = etree.SubElement(mpi, "mpiNumDomains")
mpiNumDomains.text = "0"
mpiExePath = etree.SubElement(mpi, "exePath")
mpiExePath.text = "/../"
mpiKeepDecomposedFiles = etree.SubElement(mpi, "keepDecomposedFiles")
mpiKeepDecomposedFiles.text = "0"
ddcParserNumProcessors = etree.SubElement(mpi, "ddcParserNumProcessors")
ddcParserNumProcessors.text = "0"
ddcComposerNumProcessors = etree.SubElement(mpi, "ddcComposerNumProcessors")
ddcComposerNumProcessors.text = "0"
mpiJoinerVersion = etree.SubElement(mpi, "joinerVersion")
mpiJoinerVersion.text = "1"
maxtime = etree.SubElement(mohid, "maxTime")
maxtime.text = "40000"
outputFile = etree.SubElement(mohid, "outputFile")
outputFile.text = "0"
outputPath = etree.SubElement(mohid, "outputPath")
outputPath.text = "/../"
#NOT HOW WORKS ON LINUX
exe = etree.SubElement(mohid, "exePath")


#PARAMETERS
parameters = etree.SubElement(root, "params")
model1 = etree.SubElement(parameters, "model1")
model1Name = etree.SubElement(model1, "name")
model1Name.text = "TEXT"
model1Path = etree.SubElement(model1, "path")
model1Path.text = "../"
model1MpiProcessors = etree.SubElement(model1, "mpiProcessors")
model1MpiProcessors.text = "2"
model1HasObc = etree.SubElement(model1, "hasObc")
model1HasObc.text = "1"
model1ObcSimulatedDays = etree.SubElement(model1, "obcSimulatedDays")
model1ObcSimulatedDays.text = "1"
model1ObcWorkPath = etree.SubElement(model1, "obcWorkPath")
model1ObcWorkPath.text = "../"
model1HdfReadSuffix = etree.SubElement(model1, "hdfReadSuffix")
model1HdfReadSuffix.text = "1"
model1Meteo = etree.SubElement(model1, "meteo")
#Se tiver meteo abre uma nova tag ident senao nem existe 
model1MeteoModelName = etree.SubElement(model1Meteo, "modelName")
model1MeteoModelName.text = "text"
model1MeteoSimulatedDays = etree.SubElement(model1Meteo, "simulatedDays")
model1MeteoSimulatedDays.text = "2"
model1MeteoFilenameFromModel = etree.SubElement(model1Meteo, "fileNameFromModel")
model1MeteoFilenameFromModel.text = "1"
model1MeteoWorkPath = etree.SubElement(model1Meteo, "workPath")
model1MeteoWorkPath.text="../"
model1GridFile = etree.SubElement(model1, "gridFilePath")
model1GridFile.text = "1"
model1RunId = etree.SubElement(model1, "runId")
model1RunId.text = "1"
model1Dt = etree.SubElement(model1, "dt")
model1Dt.text = "1"
model1BackupPath = etree.SubElement(model1, "backupPath")
model1BackupPath.text = "../"
model1StoragePath = etree.SubElement(model1, "storagePath")
model1StoragePath.text = "../"
model1HasWaterProperties = etree.SubElement(model1, "hasWaterProperties")
model1HasWaterProperties.text = "1"
model1HasInterfaceSedimentWater = etree.SubElement(model1, "hasInterfaceSedimentWater")
model1HasInterfaceSedimentWater.text = "1"
model1HasSurfaceHdf = etree.SubElement(model1, "hasSurfaceHdf")
model1HasSurfaceHdf.text = "1"
model1HasGotm = etree.SubElement(model1, "hasGotm")
model1HasGotm.text = "0"
model1HasOutputWindow = etree.SubElement(model1, "outputWindow")
model1HasOutputWindow.text = "2"
model1HasSolutionFromFile = etree.SubElement(model1, "hasSolutionFromFile")
model1HasSolutionFromFile.text = "1"


s = etree.tostring(root, pretty_print=True)
print s