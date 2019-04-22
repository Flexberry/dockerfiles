#!/bin/sh
if [ -n  "$BOOTUP_CHECK_URL" ]
then
  echo "Booting up apache service $BOOTUP_CHECK_URL"
  until wget -c $BOOTUP_CHECK_URL >/dev/null  2>&1
  do
    echo "Wait for start up apache service"
    sleep 0.1;
  done
  echo "Booted up apache service $BOOTUP_CHECK_URL"
else
  echo "Skipped booting up"
fi