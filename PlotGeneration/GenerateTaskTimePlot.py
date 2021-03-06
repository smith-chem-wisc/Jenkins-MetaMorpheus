import matplotlib.pyplot as pyplot
import pandas

df = pandas.read_csv("D:/Jenkins_Runs/Results/ProcessedResults.csv")

xLabel = "Date"
yLabel = "Time to run task (min)"

pyplot.xlabel(xLabel)
pyplot.ylabel(yLabel)

width=0.35

x = df[xLabel]
y1 = df["Initial Search Time"]
y2 = df["Calibration Time"]
y3 = df["Post-calibration Search Time"]
y4 = df["GPTMD Time"]
y5 = df["Post-GPTMD Search Time"]

locs = [0,1,2,3,4]
labels = [x[0].split(' ',1)[0], x[1].split(' ',1)[0], x[2].split(' ',1)[0], x[3].split(' ',1)[0], x[4].split(' ',1)[0]]
pyplot.xticks(locs, labels, fontsize=8)

b1 = pyplot.bar(x, y1, width=width, color='blue')
b2 = pyplot.bar(x, y2, width=width, bottom=y1, color='orange')
b3 = pyplot.bar(x, y3, width=width, bottom=y1+y2, color='blue')
b4 = pyplot.bar(x, y4, width=width, bottom=y1+y2+y3, color='green')
b5 = pyplot.bar(x, y5, width=width, bottom=y1+y2+y3+y4, color='blue')

pyplot.legend((b1[0], b2[0], b4[0]), ('Search', 'Calibration', 'GPTMD'), bbox_to_anchor=(1.1, 1.18), loc=1)

pyplot.savefig('D:/Jenkins_Runs/Results/TaskTimeReport.png')