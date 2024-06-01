MSBuild D:\Jenkins_Runs\Auditor\Auditor.sln /verbosity:minimal
D:\Jenkins_Runs\Auditor\Auditor\bin\Debug\Auditor -i D:\Jenkins_Runs\TestFiles\Results -o D:\Jenkins_Runs\Results -n 5
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GeneratePsmPlot.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GeneratePsmPlot_CrossLink.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GeneratePsmPlot_SemiNonModernGlyco.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GeneratePsmPlot_TopDown.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GenerateTaskTimePlot.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GenerateTaskTimePlot_SemiNonModernXLGlyco.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GenerateTaskTimePlot_TopDown.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GeneratePeptidePlot.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GeneratePeptidePlot_SemiNonModern.py D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GenerateProteoformPlot_TopDown.py D:\Jenkins_Runs\Results
