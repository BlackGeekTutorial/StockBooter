# StockBooter
Just a small tool written in vb.net that allows to tether boot ANY iOS device with the latest iOS version signed by Apple.

Requirements:
1. Windows Vista or higher
2. .NET Freamwork 4.0 or higher
3. iTunes

What is StockBooter:
StockBooter is a tool that grabs signed APTicket, devicetree and kernelcache during an iTunes restore and uses them to tether boot the attached device.
Of course you can only boot the latest stock iOS firmare (8.1.2 as of now) and you need to have the same firmware installed on the device you are booting.

Is this crap useless?
No, this is the only way to kick a device out of recovery mode in some cases.
iH8Sn0w twweted about the command 'nvram boot-command=NOT_FSBOOT' few days ago. If you run that command on your Jailbroken iOS device, it will get stuck in Recovery Mode and you won't be able to fix, even with a restore.

"The only way to get out of that was by tethered booting a stock iOS, jailbreaking and setting it back to fsboot." - iH8Sn0w said

Mac version is coming soon btw.
