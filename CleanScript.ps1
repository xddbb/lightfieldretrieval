$file = $args[0]

if ($file)
{
    # File provided
    echo("Processing file " + $file)
    $dir = (get-item $file).directoryname
    $basenames = Get-Content($file)
    # For each model delete generated files
    foreach($basemodel in $basenames)
    {    
        $modeldir = (get-item($dir + "\" + $basemodel)).directoryname
        echo("Cleaning " + $modeldir)
        remove-item($modeldir + "\*.bmp")
        remove-item($modeldir + "\*.dist")
        remove-item($modeldir + "\*.xml")
        $outfile = $dir + "\" + $basemodel + ".dist"                                
        
    }    
    echo("Done processing file " + $file)
}
else
{
    echo("No input file!")
}
