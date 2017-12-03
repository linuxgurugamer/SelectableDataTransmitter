SelectableDataTransmitter is a module which is used to allow an antenna be switchable between DIRECT mode and RELAY mode.
While an antenna is being reconfigured, the antennaPower is set to 0, so that it is active, but unable to do anything until it is fully reconfigured.
In the editor, two buttons are show,, Set DIRECT, and Set RELAY, this is so you can reference a specific setting in an action group.  In flight, only 
one button is shown.  While reconfiguration is in progress, neither is shown, but a message is shown showing the time until the reconfig is complete.

In the cfg file, there needs to be a section called ANTENNATYPE for each antenna type.  Each section needs to have all the values that a normal
antenna would have.
The module itself will need to have two lines, one specifying the defaultAntennaType, and the second specifying the time needed to reconfigure the antenna.

The cfg definition should look like the following:

	MODULE
	{
		name = SelectableDataTransmitter

		defaultAntennaType = DIRECT
		reconfigTime = 60 // seconds

		ANTENNATYPE
		{
			antennaType = DIRECT
			packetInterval = 0.03
			packetSize = 2
			packetResourceCost = 50
			requiredResource = ElectricCharge
			DeployFxModules = 0
			antennaPower = 115000000000
			ProgressFxModules = 1
			antennaCombinable = True
		}
		ANTENNATYPE
		{
			antennaType = RELAY
			packetInterval = 0.03
			packetSize = 2
			packetResourceCost = 50
			requiredResource = ElectricCharge
			DeployFxModules = 0
			antennaPower = 25000000000
			ProgressFxModules = 1
			antennaCombinable = True
		}
	}