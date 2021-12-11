# Azure Service Bus Evaluation

To run the Sender container:

```
docker run -d -t -i --name send$RANDOM \
-e ASBEConfig__SBConnectionString="Endpoint=..." \
-e ASBEConfig__QueueName="queue1" \
-e ASBEConfig__ParallelDegrees=-1 \
-e ASBEConfig__Start=0 \
-e ASBEConfig__End=5 \
-e ASBECOnfig__RunConstant=true \
ghcr.io/michaelsrichter/asbe:master
```