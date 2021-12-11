docker run -d -t -i --name send$RANDOM \
-e ASBEConfig__SBConnectionString="Endpoint=sb://sb-asbe-31784.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hY7FmbYWTuWHHjCFZBej+DXXV+SgD6IfIzz9GaYONW0=" \
-e ASBEConfig__QueueName="queue1" \
-e ASBEConfig__ParallelDegrees=-1 \
-e ASBEConfig__Start=0 \
-e ASBECOnfig__RunConstant=true \
ghcr.io/michaelsrichter/asbe:master

