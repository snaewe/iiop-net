dest=..


for name in *.html; do
	cat layout1.html $name layout2.html >> $dest/$name
done

mkdir $dest/Tutorial

for name in Tutorial/*.html; do
	cat layout1.html $name layout2.html >> $dest/$name
done

rm $dest/layout1.html $dest/layout2.html
