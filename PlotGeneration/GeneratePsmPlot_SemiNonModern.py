import matplotlib.pyplot as pyplot
import pandas

# read the csv
df = pandas.read_csv("D:/Jenkins_Runs/Results/ProcessedResults.csv")

####### Generate the Top Down Plot #######
# label the axis
xLabel = "Date"
yLabel = "PSMs"

pyplot.xlabel(xLabel)
pyplot.ylabel(yLabel)
pyplot.title('Other Search Task Results')

# generate the plot
x = df[xLabel]
y1 = df["Semispecific PSMs"]
y2 = df["Modern Search PSMs"]
y3 = df["Nonspecific PSMs"]

b1 = pyplot.plot(x, y1, color='blue')
b2 = pyplot.plot(x, y2, color='orange')
b3 = pyplot.plot(x, y3, color='green')

# set up x axis ticks
locs = [0,1,2,3,4]
labels = [x[0].split(' ',1)[0], x[1].split(' ',1)[0], x[2].split(' ',1)[0], x[3].split(' ',1)[0], x[4].split(' ',1)[0]]
pyplot.xticks(locs, labels, fontsize=8)

# set up y axis limits
ymin, ymax = pyplot.ylim()
pyplot.ylim(ymin - 1000, ymax + 1000)

# set up legend
pyplot.legend((b1[0], b2[0], b3[0]), ('Semi-Specific Search', 'Modern Search', 'Non-Specific Search'), loc=1, bbox_to_anchor=(1.1, 1.18))

# label data points
for i, txt in enumerate(y1):
    pyplot.annotate(txt, (x[i],y1[i]), fontsize=6)
for i, txt in enumerate(y2):
    pyplot.annotate(txt, (x[i],y2[i]), fontsize=6)
for i, txt in enumerate(y3):
    pyplot.annotate(txt, (x[i],y3[i]), fontsize=6)

# save the plot
pyplot.savefig('D:/Jenkins_Runs/Results/PSMReport_SemiNonModern.png')