import matplotlib.pyplot as pyplot
import pandas
import sys
import os

directory = str(sys.argv[1])
# read the csv
df = pandas.read_csv(os.path.join(directory, 'ProcessedResults.csv'))
colors = pandas.read_csv("D://Jenkins_Runs/PlotGeneration/PlotColorDict.csv")
colorDict = dict(zip(colors["Label"], colors["Color"]))

# label the axes
xLabel = "Date"
yLabel = "Peptides"

pyplot.title('Bottom-Up Peptides')
pyplot.xlabel(xLabel)
pyplot.ylabel(yLabel)

# generate the plot
x = df[xLabel]
y1 = df["Initial Search Peptides"]
y2 = df["Post-calibration Peptides"]
y3 = df["Post-GPTMD Peptides"]

b1 = pyplot.plot(x, y1, color=colorDict["Search"])
b2 = pyplot.plot(x, y2, color=colorDict["Calibrate"])
b3 = pyplot.plot(x, y3, color=colorDict["GPTMD"])

# Set up x-axis ticks
n = min(20, len(x))  # Limit to 20 ticks or the number of data points, whichever is smaller
step = len(x) // n
locs = range(0, len(x), step)
labels = [x[i].split(' ',1)[0] for i in locs]
rotation = 45 if len(x) > 10 else 0
pyplot.xticks(locs, labels, rotation=rotation, fontsize=8)

# set up y axis limits
ymin, ymax = pyplot.ylim()
pyplot.ylim(ymin - 1000, ymax + 1000)

# set up legend
pyplot.legend((b1[0], b2[0], b3[0]), ('Initial', 'Post-Calibration', 'Post-GPTMD'), loc=1, bbox_to_anchor=(1.1, 1.18))

# label data points
for i, txt in enumerate(y1):
	step = max(1, len(y1) // 10) 
	if i % step == 0:
		pyplot.annotate(txt, (x[i],y1[i]), fontsize=6)
for i, txt in enumerate(y2):
	step = max(1, len(y2) // 10) 
	if i % step == 0:
		pyplot.annotate(txt, (x[i],y2[i]), fontsize=6)
for i, txt in enumerate(y3):
	step = max(1, len(y3) // 10) 
	if i % step == 0:
		pyplot.annotate(txt, (x[i],y3[i]), fontsize=6)

# save the plot
pyplot.tight_layout()
pyplot.savefig(os.path.join(directory, 'PeptideReport.png'))