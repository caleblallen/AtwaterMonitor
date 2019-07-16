# Atwater Monitor
This is concepted as an SNMP monitor for the UPS devices employeed throughout our network.

## Design Goals

Create a program that will discover, track, and present the ambient temperature recorded by UPS temperature probes. 

The Atwater Monitor will:
1. keep a rolling average of unit temperatures
2. respond to HTTP requests with temperature information in order to integrate with a simple webapp
3. notify administrators of dangerous temperatures via email alerts

## TODO

* Implement email alerts for devices above a threshold. 
* UPS IP Addresses are hardcoded. Switch this to a config file or database.
* Sanitize web requests to prevent security breaches
* Certain methods have meaningless return values, evaluate and refactor.
* Re-Evaluate the "MonitoredDevices" List in the Model object. It is likely that upating a specific device will be more common than updating all devices. Thus O(N) will be detrimental when/if the project scales.
* Add the following metrics to tracking: 
	1. Battery Temperature
	2. Input Voltage
	3. Input Frequency
* Recognize when a device goes offline and signal users that the information on the device is stale.