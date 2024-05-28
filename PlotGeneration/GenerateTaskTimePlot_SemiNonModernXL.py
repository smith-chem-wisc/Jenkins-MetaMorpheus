import matplotlib.pyplot as pyplot
import pandas

df = pandas.read_csv("D:/Jenkins_Runs/Results/ProcessedResults.csv")

xLabel = "Date"
yLabel = "Time to run task (min)"

pyplot.title('Other Search Task Time')
pyplot.xlabel(xLabel)
pyplot.ylabel(yLabel)

width=0.35

x = df[xLabel]
y1 = df["Semispecific Search Time"]
y2 = df["Nonspecific Search Time"]
y3 = df["XL Search Time"]
y4 = df["Modern Search Time"]
pyplot.ylim(0,200)

locs = [0,1,2,3,4]
labels = [x[0].split(' ',1)[0], x[1].split(' ',1)[0], x[2].split(' ',1)[0], x[3].split(' ',1)[0], x[4].split(' ',1)[0]]
pyplot.xticks(locs, labels, fontsize=8)

b1 = pyplot.bar(x, y1, width=width, color='blue')
b2 = pyplot.bar(x, y2, width=width, bottom=y1, color='orange')
b3 = pyplot.bar(x, y3, width=width, bottom=y1+y2, color='green')
b4 = pyplot.bar(x, y4, width=width, bottom=y1+y2+y3, color='red')

pyplot.legend((b1[0], b2[0], b3[0], b4[0]), ('Semi-Specific', 'Non-Specific', 'Cross Link', 'Modern'), bbox_to_anchor=(1.1, 1.18), loc=1)

pyplot.savefig('D:/Jenkins_Runs/Results/TaskTimeReport_SemiNonModernXL.png')