# . ./root/etc/wal-g.d/server.conf
# . ./root/etc/wal-g.d/server-file.conf

# echo "$WALG_VARS";

ifs=$IFS
json='{'
n=0
for name in $WALG_VARS
do
  if [ $n -gt 0 ]
  then
    sep=','
  else
    sep=''
  fi
  eval value=\${$name}
  re='^[0-9]+$'
  if  [[ $value =~ $re ]] ; then
    #echo "$value is number"
    json="$json$sep\"$name\":$value"
  else
    json="$json$sep\"$name\":\"$value\""    
  fi
#   echo $var=$value 
  let n=$n+1
done
json="$json}"
echo $json
