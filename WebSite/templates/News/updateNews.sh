#!/bin/sh
#get the news as html and update it in the website
#

/usr/bin/wget -q -O ~/newsHtmlData.tmp "http://sourceforge.net/export/projnews.php?group_id=80227&limit=2&show_summaries=0" > /dev/null

cat newsLayout/layout1.html > newsFeed.html
sed -f sed.cmd newsHtmlData.tmp >> newsFeed.html
cat newsLayout/layout2.html >> newsFeed.html

cp -f newsFeed.html /home/groups/i/ii/iiop-net/htdocs/newsFeed.html



