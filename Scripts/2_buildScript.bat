dotnet restore -v:minimal 
dotnet msbuild D:\Jenkins_Runs\MetaMorpheus_MasterBranch\MetaMorpheus\CMD\CMD.csproj -v:minimal -p:Configuration=Release -p:UseSharedCompilation=false