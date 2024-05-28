For /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%c-%%a-%%b)
For /f "tokens=1-2 delims=/:" %%a in ("%TIME%") do (set mytime=%%a%%b)

copy "D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\settings.toml" "D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0" /y

D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0\cmd.exe -t D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\Task1SearchExample.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\Task2CalibrationExample.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\Task3SearchExample.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\Task4GptmdExample.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\Task5SearchExample.toml -s D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\04-30-13_CAST_Frac4_6uL.raw D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\04-30-13_CAST_Frac5_4uL.raw -d D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\uniprot-cRAP-1-24-2018.xml.gz D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\uniprot-mouse-reviewed-1-24-2018.xml.gz -o D:\Jenkins_Runs\TestFiles\Results\Classic_[%mydate%_%mytime%]

D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0\cmd.exe -t D:\Jenkins_Runs\TestFiles\DataAndRunSettings\XL\XLSearchTaskconfig.toml -s D:\Jenkins_Runs\TestFiles\DataAndRunSettings\XL\2017-11-21_XL_DSSO_Ribosome_RT60min_1-calib.mzml -d D:\Jenkins_Runs\TestFiles\DataAndRunSettings\XL\RibosomeGO.fasta -o D:\Jenkins_Runs\TestFiles\Results\XL_[%mydate%_%mytime%]

D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0\cmd.exe -t D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Nonspecific\SearchTaskconfig_Nonspecific.toml -s D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Nonspecific\20130504_EXQ3_MiBa_SA_Fib-2.raw -d D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Nonspecific\uniprot-filtered-reviewed_HomoSapiens.fasta -o D:\Jenkins_Runs\TestFiles\Results\Nonspecific_[%mydate%_%mytime%]

D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0\cmd.exe -t D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Semispecific\SearchTaskconfig_Semispecific.toml -s D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\04-30-13_CAST_Frac4_6uL.raw D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\04-30-13_CAST_Frac5_4uL.raw -d D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\uniprot-cRAP-1-24-2018.xml.gz D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\uniprot-mouse-reviewed-1-24-2018.xml.gz -o D:\Jenkins_Runs\TestFiles\Results\Semispecific_[%mydate%_%mytime%]

D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0\cmd.exe -t D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Modern\SearchTaskconfig_Modern.toml -s D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\04-30-13_CAST_Frac4_6uL.raw D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\04-30-13_CAST_Frac5_4uL.raw -d D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\uniprot-cRAP-1-24-2018.xml.gz D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Classic\uniprot-mouse-reviewed-1-24-2018.xml.gz -o D:\Jenkins_Runs\TestFiles\Results\Modern_[%mydate%_%mytime%]

D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0\cmd.exe -t D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\Task4-SearchTaskconfig.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\Task1-calibrateTaskconfig.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\Task4-SearchTaskconfig.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\Task2-AveragingTaskconfig.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\Task4-SearchTaskconfig.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\Task3-GPTMDTaskconfig.toml D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\Task4-SearchTaskconfig.toml -s D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\02-17-20_jurkat_td_rep2_fract4.raw D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\02-18-20_jurkat_td_rep2_fract5.raw -d D:\Jenkins_Runs\TestFiles\DataAndRunSettings\TopDown\uniprotkb_human_proteome_AND_reviewed_t_2024_03_22.xml -o D:\Jenkins_Runs\TestFiles\Results\TopDown_[%mydate%_%mytime%]

D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\bin\Release\net6.0\cmd.exe -t D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Glyco\Task1-GlycoSearchTaskconfig.toml -s D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Glyco\2019_09_16_StcEmix_35trig_EThcD25_rep1.raw -d D:\Jenkins_Runs\TestFiles\DataAndRunSettings\Glyco\MucinBackground_4mucins_FASTA.fasta -o D:\Jenkins_Runs\TestFiles\Results\Glyco_[%mydate%_%mytime%]







