import matplotlib.pyplot as pyplot
import pandas

# read the csv
df = pandas.read_csv("D:/Jenkins_Runs/Results/ProcessedResults.csv")
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

# set up x axis ticks
locs = [0,1,2,3,4]
labels = [x[0].split(' ',1)[0], x[1].split(' ',1)[0], x[2].split(' ',1)[0], x[3].split(' ',1)[0], x[4].split(' ',1)[0]]
pyplot.xticks(locs, labels, fontsize=8)

# set up y axis limits
ymin, ymax = pyplot.ylim()
pyplot.ylim(ymin - 1000, ymax + 1000)

# set up legend
pyplot.legend((b1[0], b2[0], b3[0]), ('Initial', 'Post-Calibration', 'Post-GPTMD'), loc=1, bbox_to_anchor=(1.1, 1.18))

# label data points
for i, txt in enumerate(y1):
    pyplot.annotate(txt, (x[i],y1[i]), fontsize=6)
for i, txt in enumerate(y2):
    pyplot.annotate(txt, (x[i],y2[i]), fontsize=6)
for i, txt in enumerate(y3):
    pyplot.annotate(txt, (x[i],y3[i]), fontsize=6)

# save the plot
pyplot.savefig('D:/Jenkins_Runs/Results/PeptideReport.png')