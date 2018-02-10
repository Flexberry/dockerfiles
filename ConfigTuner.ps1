Param(
	[String] $ConfigPath,
	# New ConnectionStrings
	[String] $ConnectionStringPostgres,
	[String] $ConnectionStringOracle,
	[String] $ConnectionStringMssql
)

$path = $MyInvocation.MyCommand.Path | split-path -parent
$configFullPath = $path + $ConfigPath
$doc = (Get-Content $configFullPath) -as [Xml]

$root = $doc.get_DocumentElement();
$root.connectionStrings.add[0].connectionString = $ConnectionStringPostgres;
$root.connectionStrings.add[1].connectionString = $ConnectionStringOracle;
$root.connectionStrings.add[2].connectionString = $ConnectionStringMssql;

$doc.Save($configFullPath)
