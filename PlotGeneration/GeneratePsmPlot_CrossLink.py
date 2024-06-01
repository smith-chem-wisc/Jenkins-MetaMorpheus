import matplotlib.pyplot as pyplot
import pandas
import sys
import os

directory = str(sys.argv[1])
# read the csv
df = pandas.read_csv(os.path.join(directory, 'ProcessedResults.csv'))
colors = pandas.read_csv("D://Jenkins_Runs/PlotGeneration/PlotColorDict.csv")
colorDict = dict(zip(colors["Label"], colors["Color"]))

####### Generate the Top Down Plot #######
# label the axis
xLabel = "Date"
yLabel = "PSMs"

pyplot.xlabel(xLabel)
pyplot.ylabel(yLabel)
pyplot.title('Crosslink Results')

# generate the plot
x = df[xLabel]
y1 = df["Interlink CSMs"]
y2 = df["Intralink CSMs"]
y3 = df["Loop CSMs"]
y4 = df["Crosslink Single PSMs"]
y5 = df["Deadend CSMs"]

b1 = pyplot.plot(x, y1, color=colorDict["Interlink"])
b2 = pyplot.plot(x, y2, color=colorDict["Intralink"])
b3 = pyplot.plot(x, y3, color=colorDict["Loop"])
b4 = pyplot.plot(x, y4, color=colorDict["Crosslink Single"])
b5 = pyplot.plot(x, y5, color=colorDict["Deadend"])

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
pyplot.legend((b1[0], b2[0], b3[0], b4[0], b5[0]), ('Interlink CSMs', 'Intralink CSMs', 'Loop CSMs', 'Crosslink Single PSMs', 'Deadend CSMs'), loc=1, bbox_to_anchor=(1.1, 1.18))

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
for i, txt in enumerate(y4):
	step = max(1, len(y4) // 10) 
	if i % step == 0:
		pyplot.annotate(txt, (x[i],y4[i]), fontsize=6)
for i, txt in enumerate(y5):
	step = max(1, len(y5) // 10) 
	if i % step == 0:
		pyplot.annotate(txt, (x[i],y5[i]), fontsize=6)


# save the plot
pyplot.tight_layout()
pyplot.savefig(os.path.join(directory, 'PSMReport_CrossLink.png'))