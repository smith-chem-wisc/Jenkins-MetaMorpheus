import matplotlib.pyplot as pyplot
import pandas

df = pandas.read_csv("D:/Jenkins_Runs/Results/ProcessedResults.csv")
colors = pandas.read_csv("D://Jenkins_Runs/PlotGeneration/PlotColorDict.csv")
colorDict = dict(zip(colors["Label"], colors["Color"]))

xLabel = "Date"
yLabel = "Time to run task (min)"

pyplot.title('Top-Down Task Time')
pyplot.xlabel(xLabel)
pyplot.ylabel(yLabel)

width=0.35

x = df[xLabel]
y1 = df["TopDown Initial Search Time"]
y2 = df["TopDown Calibration Time"]
y3 = df["TopDown Post-calibration Search Time"]
y4 = df["TopDown Averaging Time"]
y5 = df["TopDown Post-averaging Search Time"]
y6 = df["TopDown GPTMD Time"]
y7 = df["TopDown Post-GPTMD Search Time"]

locs = [0,1,2,3,4]
labels = [x[0].split(' ',1)[0], x[1].split(' ',1)[0], x[2].split(' ',1)[0], x[3].split(' ',1)[0], x[4].split(' ',1)[0]]
pyplot.xticks(locs, labels, fontsize=8)

b1 = pyplot.bar(x, y1, width=width, color=colorDict["Search"])
b2 = pyplot.bar(x, y2, width=width, bottom=y1, color=colorDict["Calibrate"])
b3 = pyplot.bar(x, y3, width=width, bottom=y1+y2, color=colorDict["Search"])
b4 = pyplot.bar(x, y4, width=width, bottom=y1+y2+y3, color=colorDict["Average"])
b5 = pyplot.bar(x, y5, width=width, bottom=y1+y2+y3+y4, color=colorDict["Search"])
b6 = pyplot.bar(x, y6, width=width, bottom=y1+y2+y3+y4+y5, color=colorDict["GPTMD"])
b7 = pyplot.bar(x, y7, width=width, bottom=y1+y2+y3+y4+y5+y6, color=colorDict["Search"])

pyplot.legend((b1[0], b2[0], b4[0], b6[0]), ('Search', 'Calibration', 'Averaging', 'GPTMD'), bbox_to_anchor=(1.1, 1.18), loc=1)

pyplot.savefig('D:/Jenkins_Runs/Results/TaskTimeReport_TopDown.png')