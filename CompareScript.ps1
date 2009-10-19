$file = $args[0]

#Const path, redo if needed
Set-Alias comparator D:\Development\Uni\Lightfieldretrieval\FeatureComparator\bin\Debug\FeatureComparator.exe

if ($file)
{
    # File provided
    echo("Processing file " + $file)
    $dir = (get-item $file).directoryname
    $basenames = Get-Content($file)
    # For each model, compare with all other models
    foreach($basemodel in $basenames)
    {    
        $basefeature = (get-item($dir + "\" + $basemodel)).directoryname + "\features.xml"
        $outfile = $dir + "\" + $basemodel + ".dist"
        Clear-Content $outfile
        foreach($model in $basenames)
        {
            $feature = (get-item($dir + "\" + $model)).directoryname + "\features.xml"
            echo("Comparing " + $basefeature + " and " + $feature)
            (comparator $basefeature $feature) + " " + $model | out-file -filepath $outfile -append
        }        
        
        
        #echo("Done rendering" + $modelpath)
        
    }    
    echo("Done processing file " + $file)
}
else
{
    echo("No input file!")
}
