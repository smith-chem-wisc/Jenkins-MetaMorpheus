MSBuild D:\Jenkins_Runs\Auditor\Auditor.sln /verbosity:minimal
D:\Jenkins_Runs\Auditor\Auditor\bin\Debug\Auditor -i D:\Jenkins_Runs\TestFiles\Results -o D:\Jenkins_Runs\Results
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GeneratePsmPlot.py
C:\Python27\python D:\Jenkins_Runs\PlotGeneration\GenerateTaskTimePlot.py