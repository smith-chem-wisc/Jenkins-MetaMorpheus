import matplotlib.pyplot as pyplot
import pandas
import sys
import os

directory = str(sys.argv[1])
# read the csv
df = pandas.read_csv(os.path.join(directory, 'ProcessedResults.csv'))
colors = pandas.read_csv("D://Jenkins_Runs/PlotGeneration/PlotColorDict.csv")
colorDict = dict(zip(colors["Label"], colors["Color"]))

xLabel = "Date"
yLabel = "Time to run task (min)"

pyplot.title('Bottom-Up Task Time')
pyplot.xlabel(xLabel)
pyplot.ylabel(yLabel)

width=0.35

x = df[xLabel]
y1 = df["Initial Search Time"]
y2 = df["Calibration Time"]
y3 = df["Post-calibration Search Time"]
y4 = df["GPTMD Time"]
y5 = df["Post-GPTMD Search Time"]

# Set up x-axis ticks
n = min(20, len(x))  # Limit to 20 ticks or the number of data points, whichever is smaller
step = len(x) // n
locs = range(0, len(x), step)
labels = [x[i].split(' ',1)[0] for i in locs]
rotation = 45 if len(x) > 10 else 0
pyplot.xticks(locs, labels, rotation=rotation, fontsize=8)

b1 = pyplot.bar(x, y1, width=width, color=colorDict["Search"])
b2 = pyplot.bar(x, y2, width=width, bottom=y1, color=colorDict["Calibrate"])
b3 = pyplot.bar(x, y3, width=width, bottom=y1+y2, color=colorDict["Search"])
b4 = pyplot.bar(x, y4, width=width, bottom=y1+y2+y3, color=colorDict["GPTMD"])
b5 = pyplot.bar(x, y5, width=width, bottom=y1+y2+y3+y4, color=colorDict["Search"])

pyplot.legend((b1[0], b2[0], b4[0]), ('Search', 'Calibration', 'GPTMD'), bbox_to_anchor=(1.1, 1.18), loc=1)

pyplot.savefig(os.path.join(directory, 'TaskTimeReport.png'))