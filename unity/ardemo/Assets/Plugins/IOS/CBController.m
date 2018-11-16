//
//  CBController.m
//  BLETR
//
//  Created by user D500 on 12/2/15.
//  Copyright (c) 2012年 __MyCompanyName__. All rights reserved.
//

#import "CBController.h"
#import "MyPeripheral.h"
#import "Globle.h"

@implementation CBController
@synthesize delegate;
@synthesize devicesList;
@synthesize isBlueOpenFlag;

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
        if (floor(NSFoundationVersionNumber) > NSFoundationVersionNumber_iOS_6_1) {
            self.edgesForExtendedLayout = UIRectEdgeNone;
        }
        devicesList = [[NSMutableArray alloc] init];
        _connectedPeripheralList = [[NSMutableArray alloc] init];
        _transServiceUUID = nil;
        _transTxUUID = nil;
        _transRxUUID = nil;
        _disUUID1 = nil;
        _disUUID2 = nil;
    }
    return self;
}

- (void)didReceiveMemoryWarning
{
    // Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
    
    // Release any cached data, images, etc that aren't in use.
}

#pragma mark - View lifecycle

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    if (floor(NSFoundationVersionNumber) > NSFoundationVersionNumber_iOS_6_1) {
        self.edgesForExtendedLayout = UIRectEdgeNone;
    }
    devicesList = [[NSMutableArray alloc] init];
    _connectedPeripheralList = [[NSMutableArray alloc] init];
    _transServiceUUID = nil;
    _transTxUUID = nil;
    _transRxUUID = nil;
    _disUUID1 = nil;
    _disUUID2 = nil;
	connectTimer = nil;
    // Do any additional setup after loading the view from its nib.
    manager = [[CBCentralManager alloc] initWithDelegate:self queue:nil];
    /*manager = [CBCentralManager alloc];
    if ([manager respondsToSelector:@selector(initWithDelegate:queue:options:)]) {
        manager = [manager initWithDelegate:self queue:nil options:@{CBCentralManagerOptionRestoreIdentifierKey: ISSC_RestoreIdentifierKey}];
    }
    else {
        manager = [manager initWithDelegate:self queue:nil];
    }*/
}

- (void)viewDidUnload
{
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    // Return YES for supported orientations
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}


- (void)dealloc {
    //[manager release];
    for (MyPeripheral *p in devicesList) {
        [self disconnectDevice:p];
    }
    //[devicesList release];
    //[super dealloc];
}

- (void) startScan 
{
    if (isBlueOpenFlag) {
        logInfo(@"[CBController] start scan");
        [manager scanForPeripheralsWithServices:nil options:nil];
        [devicesList removeAllObjects];
    }
	else
	{
		isBlueOpenFlag = [self isLECapableHardware];
	}
}

/*
 Request CBCentralManager to stop scanning for heart rate peripherals
 */
- (void) stopScan 
{
    logInfo(@"[CBController] stop scan");
    [manager stopScan];
}

- (NSMutableData *) hexStrToData: (NSString *)hexStr
{
    NSMutableData *data= [[NSMutableData alloc] init];
    NSUInteger len = [hexStr length];
    
    unsigned char whole_byte;
    char byte_chars[3] = {'\0','\0','\0'};
    int i;
    for (i=0; i < len/2; i++) {
        byte_chars[0] = [hexStr characterAtIndex:i*2];
        byte_chars[1] = [hexStr characterAtIndex:i*2+1];
        whole_byte = strtol(byte_chars, NULL, 16);
        [data appendBytes:&whole_byte length:1]; 
    }
    //return [data autorelease];
    return data;
}

- (void) ConnectDevice:(NSString *) mac
{
    MyPeripheral *myPeripheral = nil;
    for (uint8_t i = 0; i < [devicesList count]; i++) {
        myPeripheral = [devicesList objectAtIndex:i];
        if ([myPeripheral.advName isEqualToString: mac]) {
            break;
        }
        myPeripheral = nil;
    }
    if (nil != myPeripheral)
    {
        [self connectDevice:myPeripheral];
    }
    else
    {
        logInfo(@"ConnectDevice mac = %@ not found", mac);
        ConnectResultAck * result = [[ConnectResultAck alloc] init];
        result.result = false;
        result.pPeripheral = [[MyPeripheral alloc] init];
        result.pPeripheral.advName = mac;
        [[NSNotificationCenter defaultCenter] postNotificationName:ConnectResult object:nil userInfo:
         [NSDictionary dictionaryWithObject:result forKey:ConnectResult]];    }
}
- (void) DisConnectDevice
{
	MyPeripheral *myPeripheral = nil;
    for (uint8_t i = 0; i < [_connectedPeripheralList count]; i++) {
        myPeripheral = [_connectedPeripheralList objectAtIndex:i];
        if (nil != myPeripheral)
        {
            [self disconnectDevice:myPeripheral];
        }
        else
        {
            logInfo(@"DisConnectDevice mac = %@ not found", myPeripheral.advName);
        }
    }
}

//timer????
-(void)connectTimeout:(NSTimer *)timer{
	MyPeripheral * peripheral = [timer userInfo];
	ConnectResultAck * result = [[ConnectResultAck alloc] init];
    result.result = false;
    result.pPeripheral = peripheral;
    [[NSNotificationCenter defaultCenter] postNotificationName:ConnectResult object:nil userInfo:
         [NSDictionary dictionaryWithObject:result forKey:ConnectResult]];
		 
	if (nil != connectTimer)
	{
		[connectTimer invalidate];//???? 
		connectTimer = nil;
	}
}

- (void)connectDevice:(MyPeripheral *) myPeripheral{
    //logInfo(@"[CBController] connectDevice: %@", myPeripheral.advName);
    [self stopScan];
    if (myPeripheral.connectStaus != MYPERIPHERAL_CONNECT_STATUS_IDLE)
        return;
    [manager connectPeripheral:myPeripheral.peripheral options:nil];  //connect to device
	if (nil != connectTimer)
	{
		[connectTimer invalidate];
		//[connectTimer release];
		connectTimer = nil;
	}
    connectTimer = [NSTimer scheduledTimerWithTimeInterval:10.0f target:self selector:@selector(connectTimeout:) userInfo:myPeripheral repeats:NO];
}

-(void)cancelConnection{
    MyPeripheral *myPeripheral = nil;
    for (uint8_t i = 0; i < [_connectedPeripheralList count]; i++) {
        myPeripheral = [_connectedPeripheralList objectAtIndex:i];
        if (nil != myPeripheral)
        {
            [manager cancelPeripheralConnection:myPeripheral.peripheral];
        }
    }
}


- (void)disconnectDevice: (MyPeripheral *)myPeripheral {
    //logInfo(@"[CBController] disconnectDevice");
    myPeripheral.canSendData = NO;
    [myPeripheral setTransDataNotification:NO];
    
    
    if ([myPeripheral.transmit canDisconnect]) {
        [manager cancelPeripheralConnection: myPeripheral.peripheral];
    }
     else {
        dispatch_async(dispatch_queue_create("temp", NULL), ^{
            //logInfo(@"[CBController] disconnectDevice : Wait for data clear");
            int count = 0;
            while (![myPeripheral.transmit canDisconnect] || myPeripheral.isNotifying) {
                //[NSThread sleepForTimeInterval:0.1];
                sleep(1);
                count++;
                if (count >= 10) {
                    break;
                }
            }
            //dispatch_async(dispatch_get_main_queue(), ^{
                [manager cancelPeripheralConnection: myPeripheral.peripheral];
            //});
        });
    }
}


- (void)updateDiscoverPeripherals {
    
}

- (void)updateMyPeripheralForNewConnected:(MyPeripheral *)myPeripheral {
    ConnectResultAck * result = [[ConnectResultAck alloc] init];
    result.result = true;
    result.pPeripheral = myPeripheral;
    [[NSNotificationCenter defaultCenter] postNotificationName:ConnectResult object:nil userInfo:
     [NSDictionary dictionaryWithObject:result forKey:ConnectResult]];
}

- (void)updateMyPeripheralForDisconnect:(MyPeripheral *)myPeripheral {
    
}

- (void)addDiscoverPeripheral:(CBPeripheral *)aPeripheral advName:(NSString *)advName RSSI:(NSNumber *)rssi{
    if(aPeripheral.name==nil)return;
    
    MyPeripheral *myPeripheral = nil;
    for (uint8_t i = 0; i < [devicesList count]; i++) {
        myPeripheral = [devicesList objectAtIndex:i];
        if (myPeripheral.peripheral == aPeripheral) {
            myPeripheral.advName = advName;			
            break;
        }
        myPeripheral = nil;
    }
    
    if (myPeripheral == nil) {
        
        myPeripheral = [[MyPeripheral alloc] init];
        myPeripheral.peripheral = aPeripheral;
        myPeripheral.advName = advName;		
        [devicesList addObject:myPeripheral];
        
        NSString *peripheralName=aPeripheral.name;
        NSRange range=[peripheralName rangeOfString:@"\n"];
        if(range.location!=NSNotFound)
        {
            peripheralName=[peripheralName stringByReplacingOccurrencesOfString:@"\n" withString:@""];
        }
        else
        {
            range=[peripheralName rangeOfString:@"\\n"];
            if(range.location!=NSNotFound)
            {
                peripheralName=[peripheralName stringByReplacingOccurrencesOfString:@"\\n" withString:@""];
            }
        }
        
        NSString * mac = [NSString stringWithFormat:@"%@\n%@\n%@", peripheralName, advName, rssi];
		logInfo(@"[CBController] addDiscoverPeripheral mac = %@", mac);
        [[NSNotificationCenter defaultCenter] postNotificationName:OnFoundDevice object:nil userInfo:
         [NSDictionary dictionaryWithObject:mac forKey:OnFoundDevice]];
        logInfo(@"[CBController] addDiscoverPeripheral post notify name:%@",OnFoundDevice);
    }
    //logInfo(@"[CBController] deviceList count = %d", [devicesList count]);
    [self updateDiscoverPeripherals];
}

- (void)storeMyPeripheral: (CBPeripheral *)aPeripheral {
    MyPeripheral *myPeripheral = nil;
    bool b = FALSE;
    for (uint8_t i = 0; i < [devicesList count]; i++) {
        myPeripheral = [devicesList objectAtIndex:i];
        if (myPeripheral.peripheral == aPeripheral) {
            b = TRUE;
            //logInfo(@"storeMyPeripheral 1");
            break;
        }
    }
    if(!b) {
        //logInfo(@"storeMyPeripheral 2");
        myPeripheral = [[MyPeripheral alloc] init];
        myPeripheral.peripheral = aPeripheral;
#if __IPHONE_OS_VERSION_MAX_ALLOWED > __IPHONE_8_4
        myPeripheral.advName = aPeripheral.identifier.UUIDString;
#else
        myPeripheral.advName = aPeripheral.UUID.UUIDString;
#endif
    }
    myPeripheral.connectStaus = MYPERIPHERAL_CONNECT_STATUS_CONNECTED;
    [_connectedPeripheralList addObject:myPeripheral];
}

- (MyPeripheral *)retrieveMyPeripheral:(CBPeripheral *)aPeripheral {
    MyPeripheral *myPeripheral = nil;
    for (uint8_t i = 0; i < [_connectedPeripheralList count]; i++) {
        myPeripheral = [_connectedPeripheralList objectAtIndex:i];
        if (myPeripheral.peripheral == aPeripheral) {
            break;
        }
    }
    return myPeripheral;
}

- (void)removeMyPeripheral: (CBPeripheral *) aPeripheral {
	if (nil != connectTimer)
	{
		[connectTimer invalidate];//???? 
		//[connectTimer release];
		connectTimer = nil;
	}
    MyPeripheral *myPeripheral = nil;
    for (uint8_t i = 0; i < [_connectedPeripheralList count]; i++) {
        myPeripheral = [_connectedPeripheralList objectAtIndex:i];
        if (myPeripheral.peripheral == aPeripheral) {
            myPeripheral.connectStaus = MYPERIPHERAL_CONNECT_STATUS_IDLE;
            [self updateMyPeripheralForDisconnect:myPeripheral];
            [_connectedPeripheralList removeObject:myPeripheral];
            [[NSNotificationCenter defaultCenter] postNotificationName:DicBlueConnect object:nil userInfo:
             [NSDictionary dictionaryWithObject:myPeripheral.advName forKey:DicBlueConnect]];
            return;
        }
    }
    for (uint8_t i = 0; i < [devicesList count]; i++) {
        myPeripheral = [devicesList objectAtIndex:i];
        if (myPeripheral.peripheral == aPeripheral) {
            myPeripheral.connectStaus = MYPERIPHERAL_CONNECT_STATUS_IDLE;
            [self updateMyPeripheralForDisconnect:myPeripheral];
            ConnectResultAck * result = [[ConnectResultAck alloc] init];
            result.result = false;
            result.pPeripheral = myPeripheral;
            [[NSNotificationCenter defaultCenter] postNotificationName:ConnectResult object:nil userInfo:
             [NSDictionary dictionaryWithObject:result forKey:ConnectResult]];
            break;
        }
    }
}

- (void)configureTransparentServiceUUID: (NSString *)serviceUUID txUUID:(NSString *)txUUID rxUUID:(NSString *)rxUUID {
    if (serviceUUID) {
        _transServiceUUID = [CBUUID UUIDWithString:serviceUUID];
        //[_transServiceUUID retain];
        //_transTxUUID = [CBUUID UUIDWithString:txUUID];
        //[_transTxUUID retain];
        //_transRxUUID = [CBUUID UUIDWithString:rxUUID];
        //[_transRxUUID retain];
    }
    else {
        _transServiceUUID = nil;
        _transTxUUID = nil;
        _transRxUUID = nil;
    }
}

- (void)configureDeviceInformationServiceUUID:(NSString *)UUID1 UUID2:(NSString *)UUID2{
    if (UUID1 || UUID2) {
        if (UUID1 != nil) {
            //_disUUID1 = [CBUUID UUIDWithString:UUID1];
            //[_disUUID1 retain];
        }
        else _disUUID1 = nil;
        
        if (UUID2 != nil) {
            //_disUUID2 = [CBUUID UUIDWithString:UUID2];
            //[_disUUID2 retain];
        }
        else _disUUID2 = nil;
    }
    else {
        _disUUID1 = nil;
        _disUUID2 = nil;
    }
}

/*
 Uses CBCentralManager to check whether the current platform/hardware supports Bluetooth LE. An alert is raised if Bluetooth LE is not enabled or is not supported.
 */
- (BOOL) isLECapableHardware
{
    NSString * state = nil;
    
    switch ([manager state]) 
    {
        case CBCentralManagerStateUnsupported:
            state = @"CBCentralManagerStateUnsupported";
            break;
        case CBCentralManagerStateUnauthorized:
            state = @"CBCentralManagerStateUnauthorized";
            break;
        case CBCentralManagerStatePoweredOff:
            state = @"Bluetooth powered off";
            break;
        case CBCentralManagerStatePoweredOn:
            logInfo(@"Bluetooth power on");
            return TRUE;
        case CBCentralManagerStateUnknown:
        default:
            return FALSE;
            
    }
    
    //logInfo(@"Central manager state: %@", state);
    
    
    //UIAlertView *alertView = [[UIAlertView alloc] initWithTitle:NSLocalizedString(@"hint", nil)  message:state delegate:self cancelButtonTitle:@"Cancel" otherButtonTitles: nil];
    //[alertView show];
    //[alertView release];
    return FALSE;
}

#pragma mark - CBCentralManager delegate methods
/*
 Invoked whenever the central manager's state is updated.
 */
- (void) centralManagerDidUpdateState:(CBCentralManager *)central 
{
    isBlueOpenFlag = [self isLECapableHardware];
}

- (void) centralManager:(CBCentralManager *)central willRestoreState:(NSDictionary *)dict {
    logInfo(@"willRestoreState %@",[dict description]);
}

/*
 Invoked when the central discovers heart rate peripheral while scanning.
 */
- (void) centralManager:(CBCentralManager *)central didDiscoverPeripheral:(CBPeripheral *)aPeripheral advertisementData:(NSDictionary *)advertisementData RSSI:(NSNumber *)RSSI 
{
    NSString * mac = nil;
#if __IPHONE_OS_VERSION_MAX_ALLOWED > __IPHONE_8_4
    mac = aPeripheral.identifier.UUIDString;
    logInfo(@"[CBController] didDiscoverPeripheral, %@, advertisementData count=%lu, RSSI=%d, devicesList count=%ld", aPeripheral.identifier, [advertisementData count], [RSSI intValue], [devicesList count]);
#else
    mac = aPeripheral.UUID.UUIDString;
    logInfo(@"[CBController] didDiscoverPeripheral, %@, advertisementData count=%d, RSSI=%d, devicesList count=%d", aPeripheral.UUID, [advertisementData count], [RSSI intValue], [devicesList count]);
    
#endif
    
    NSArray *advDataArray = [advertisementData allValues];
    NSArray *advValueArray = [advertisementData allKeys];
    
    for (int i=0; i < [advertisementData count]; i++)
    {
        logInfo(@"adv data=%@, %@ ", [advDataArray objectAtIndex:i], [advValueArray objectAtIndex:i]);
    }
    [self addDiscoverPeripheral:aPeripheral advName:mac RSSI:RSSI];
}

/*
 Invoked when the central manager retrieves the list of known peripherals.
 Automatically connect to first known peripheral
 */
- (void)centralManager:(CBCentralManager *)central didRetrievePeripherals:(NSArray *)peripherals
{
    logInfo(@"Retrieved peripheral: %u - %@", [peripherals count], peripherals);
    if([peripherals count] >=1)
    {
        [self connectDevice:[peripherals objectAtIndex:0]];
    }
}

/*
 Invoked whenever a connection is succesfully created with the peripheral. 
 Discover available services on the peripheral
 */
- (void) centralManager:(CBCentralManager *)central didConnectPeripheral:(CBPeripheral *)aPeripheral 
{
    if (nil != connectTimer)
	{
		[connectTimer invalidate];//???? 
		//[connectTimer release];
		connectTimer = nil;
	}
#if __IPHONE_OS_VERSION_MAX_ALLOWED > __IPHONE_8_4
    
    logInfo(@"[CBController] didConnectPeripheral, uuid=%@", aPeripheral.identifier);
    
#else
    
    logInfo(@"[CBController] didConnectPeripheral, uuid=%@", aPeripheral.UUID);
    
#endif
    

    [aPeripheral setDelegate:self];

    [self storeMyPeripheral:aPeripheral];
    
    isISSCPeripheral = FALSE;
    NSMutableArray *uuids = [[NSMutableArray alloc] initWithObjects:[CBUUID UUIDWithString:UUIDSTR_DEVICE_INFO_SERVICE], [CBUUID UUIDWithString:UUIDSTR_ISSC_PROPRIETARY_SERVICE], nil];
    if (_transServiceUUID)
        [uuids addObject:_transServiceUUID];
    [aPeripheral discoverServices:uuids];
    //[uuids release];
}

/*
 Invoked whenever an existing connection with the peripheral is torn down. 
 Reset local variables
 */
- (void)centralManager:(CBCentralManager *)central didDisconnectPeripheral:(CBPeripheral *)aPeripheral error:(NSError *)error
{
#if __IPHONE_OS_VERSION_MAX_ALLOWED > __IPHONE_8_4
    
    logInfo(@"[CBController] didDisonnectPeripheral uuid = %@, error msg:%ld, %@, %@", aPeripheral.identifier, error.code ,[error localizedFailureReason], [error localizedDescription]);
#else
    
    logInfo(@"[CBController] didDisonnectPeripheral uuid = %@, error msg:%ld, %@, %@", aPeripheral.UUID, error.code ,[error localizedFailureReason], [error localizedDescription]);
    
#endif
    [self removeMyPeripheral:aPeripheral];
}

/*
 Invoked whenever the central manager fails to create a connection with the peripheral.
 */
- (void)centralManager:(CBCentralManager *)central didFailToConnectPeripheral:(CBPeripheral *)aPeripheral error:(NSError *)error
{
    logInfo(@"[CBController] Fail to connect to peripheral: %@ with error = %@", aPeripheral, [error localizedDescription]);
    [self removeMyPeripheral:aPeripheral];
}

#pragma mark - CBPeripheral delegate methods
/*
 Invoked upon completion of a -[discoverServices:] request.
 Discover available characteristics on interested services
 */
- (void) peripheral:(CBPeripheral *)aPeripheral didDiscoverServices:(NSError *)error 
{
    for (CBService *aService in aPeripheral.services) 
    {
        logInfo(@"[CBController] Service found with UUID: %@", aService.UUID);
      //  NSArray *uuids = [[NSArray alloc] initWithObjects:[CBUUID UUIDWithString:@"2A4D"], nil];
        [aPeripheral discoverCharacteristics:nil forService:aService];
      //  [uuids release];
    }
}

/*
 Invoked upon completion of a -[discoverCharacteristics:forService:] request.
 Perform appropriate operations on interested characteristics
 */
- (void) peripheral:(CBPeripheral *)aPeripheral didDiscoverCharacteristicsForService:(CBService *)service error:(NSError *)error
{
    logInfo(@"\n[CBController] didDiscoverCharacteristicsForService: %@", service.UUID);
    CBCharacteristic *aChar = nil;
    MyPeripheral *myPeripheral = [self retrieveMyPeripheral:aPeripheral];
    if (myPeripheral == nil) {
        return;
    }

    if (_transServiceUUID && [service.UUID isEqual:_transServiceUUID]) {
        isISSCPeripheral = TRUE;
        myPeripheral.canSendData = YES;
        for (aChar in service.characteristics)
        {
            if ([aChar.UUID isEqual:_transRxUUID]) {
                [myPeripheral setTransparentDataWriteChar:aChar];
                logInfo(@"found custom TRANS_RX");
            }
            else if ([aChar.UUID isEqual:_transTxUUID]) {
                logInfo(@"found custome TRANS_TX");
                [myPeripheral setTransparentDataReadChar:aChar];
              //  [aPeripheral setNotifyValue:TRUE forCharacteristic:aChar];
            }
        }
    }
    else if ([service.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_ISSC_PROPRIETARY_SERVICE]]) {
        isISSCPeripheral = TRUE;
        myPeripheral.canSendData = YES;
        for (aChar in service.characteristics)
        {
            if ((_transServiceUUID == nil) && [aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_ISSC_TRANS_RX]]) {
                [myPeripheral setTransparentDataWriteChar:aChar];
                logInfo(@"found TRANS_RX");
                
            }
            else if ((_transServiceUUID == nil) && [aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_ISSC_TRANS_TX]]) {
                 logInfo(@"found TRANS_TX");
                [myPeripheral setTransparentDataReadChar:aChar];
                //[aPeripheral setNotifyValue:TRUE forCharacteristic:aChar];
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_CONNECTION_PARAMETER_CHAR]]) {
                [myPeripheral setConnectionParameterChar:aChar];
                 logInfo(@"found CONNECTION_PARAMETER_CHAR");
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_AIR_PATCH_CHAR]]) {
                [myPeripheral setAirPatchChar:aChar];
                logInfo(@"found UUIDSTR_AIR_PATCH_CHAR");
                [myPeripheral.transmit enableReliableBurstTransmit:myPeripheral.peripheral andAirPatchCharacteristic:myPeripheral.airPatchChar];
            }
        }
    }
    else if([service.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_DEVICE_INFO_SERVICE]]) {

        for (aChar in service.characteristics)
        {
            if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_MANUFACTURE_NAME_CHAR]]) {
                [myPeripheral setManufactureNameChar:aChar];
                logInfo(@"found manufacture name char");
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_MODEL_NUMBER_CHAR]]) {
                [myPeripheral setModelNumberChar:aChar];
                    logInfo(@"found model number char");

            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_SERIAL_NUMBER_CHAR]]) {
                [myPeripheral setSerialNumberChar:aChar];
                logInfo(@"found serial number char");
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_HARDWARE_REVISION_CHAR]]) {
                [myPeripheral setHardwareRevisionChar:aChar];
                logInfo(@"found hardware revision char");
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_FIRMWARE_REVISION_CHAR]]) {
                [myPeripheral setFirmwareRevisionChar:aChar];
                logInfo(@"found firmware revision char");
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_SOFTWARE_REVISION_CHAR]]) {
                [myPeripheral setSoftwareRevisionChar:aChar];
                logInfo(@"found software revision char");
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_SYSTEM_ID_CHAR]]) {
                [myPeripheral setSystemIDChar:aChar];
                logInfo(@"[CBController] found system ID char");
            }
            else if ([aChar.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_IEEE_11073_20601_CHAR]]) {
                [myPeripheral setCertDataListChar:aChar];
                logInfo(@"found certification data list char");
            }
            else if (_disUUID1 && [aChar.UUID isEqual:_disUUID1]) {
                [myPeripheral setSpecificChar1:aChar];
                logInfo(@"found specific char1");
            }
            else if (_disUUID2 && [aChar.UUID isEqual:_disUUID2]) {
                [myPeripheral setSpecificChar2:aChar];
                logInfo(@"found specific char2");
            }
        }
    }
    
    if (isISSCPeripheral == TRUE) {
        [self updateMyPeripheralForNewConnected:myPeripheral];
    }
	else
	{
		/*ConnectResultAck * result = [[ConnectResultAck alloc] init];
		result.result = false;
		result.pPeripheral = myPeripheral;
		[[NSNotificationCenter defaultCenter] postNotificationName:ConnectResult object:nil userInfo:
         [NSDictionary dictionaryWithObject:result forKey:ConnectResult]];*/
	}
}

/*
 Invoked upon completion of a -[readValueForCharacteristic:] request or on the reception of a notification/indication.
 */
- (void) peripheral:(CBPeripheral *)aPeripheral didUpdateValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error 
{
    MyPeripheral *myPeripheral = [self retrieveMyPeripheral:aPeripheral];
    if (myPeripheral == nil) {
        return;
    }
    //logInfo(@"[CBController] didUpdateValueForCharacteristic %@",[characteristic  value]);
    
    if ([characteristic.service.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_DEVICE_INFO_SERVICE]]) {
        if (myPeripheral.deviceInfoDelegate == nil)
            return;
        if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_MANUFACTURE_NAME_CHAR]]) {
            logInfo(@"[CBController] update manufacture name");
            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateManufactureName:error:)]) {
                [[myPeripheral deviceInfoDelegate] MyPeripheral:myPeripheral didUpdateManufactureName:[[NSString alloc] initWithData:characteristic.value encoding:NSUTF8StringEncoding] error:error];
            }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_MODEL_NUMBER_CHAR]]) {
            logInfo(@"[CBController] update model number");

            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateModelNumber:error:)]) {
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateModelNumber:[[NSString alloc] initWithData:characteristic.value encoding:NSUTF8StringEncoding] error:error];
            }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_SERIAL_NUMBER_CHAR]]) {
            logInfo(@"[CBController] update serial number");
            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateSerialNumber:error:)]) {
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateSerialNumber:[[NSString alloc] initWithData:characteristic.value encoding:NSUTF8StringEncoding] error:error];
            }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_HARDWARE_REVISION_CHAR]]) {
            logInfo(@"[CBController] update hardware revision");

            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateHardwareRevision:error:)]){
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateHardwareRevision:[[NSString alloc] initWithData:characteristic.value encoding:NSUTF8StringEncoding] error:error];
            }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_FIRMWARE_REVISION_CHAR]]) {
            logInfo(@"[CBController] update firmware revision");

            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateFirmwareRevision:error:)]){
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateFirmwareRevision:[[NSString alloc] initWithData:characteristic.value encoding:NSUTF8StringEncoding] error:error];
            }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_SOFTWARE_REVISION_CHAR]]) {

            logInfo(@"[CBController] update software revision");

            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateSoftwareRevision:error:)]){
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateSoftwareRevision:[[NSString alloc] initWithData:characteristic.value encoding:NSUTF8StringEncoding] error:error];
            }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_SYSTEM_ID_CHAR]]) {
            logInfo(@"[CBController] update system ID");

            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateSystemId:error:)]){
                
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateSystemId:characteristic.value error:error];
                
            }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_IEEE_11073_20601_CHAR]]) {
            logInfo(@"[CBController] update IEEE_11073_20601: %@",characteristic.value);
            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateIEEE_11073_20601:error:)]){
                
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateIEEE_11073_20601:characteristic.value error:error];
                
            }
        }
        else if (_disUUID1 && [characteristic.UUID isEqual:_disUUID1]) {
            logInfo(@"[CBController] update specific UUID 1: %@",characteristic.value);
            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateSpecificUUID1:error:)]){
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateSpecificUUID1:characteristic.value error:error];
            }
        }
        else if (_disUUID2 && [characteristic.UUID isEqual:_disUUID2]) {
            logInfo(@"[CBController] update specific UUID 2: %@",characteristic.value);
            
            if ([(NSObject *)myPeripheral.deviceInfoDelegate respondsToSelector:@selector(MyPeripheral:didUpdateSpecificUUID2:error:)]){
                [myPeripheral.deviceInfoDelegate MyPeripheral:myPeripheral didUpdateSpecificUUID2:characteristic.value error:error];
            }
        }
    }
    else if ([characteristic.service.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_ISSC_PROPRIETARY_SERVICE]]) {
        if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_CONNECTION_PARAMETER_CHAR]]) {
            logInfo(@"[CBController] update connection parameter: %@", characteristic.value);
            unsigned char buf[10];
            CONNECTION_PARAMETER_FORMAT *parameter;
            
            [characteristic.value getBytes:&buf[0] length:sizeof(CONNECTION_PARAMETER_FORMAT)];
            parameter = (CONNECTION_PARAMETER_FORMAT *)&buf[0];

            //logInfo(@"[CBController] %02X, %02x, %02x, %02x, %02X, %02x, %02x, %02x, %02x,status= %d, min= %f,max= %f, latency=%d, timeout=%d", buf[0],buf[1],buf[2],buf[3],buf[4],buf[5],buf[6],buf[7],buf[8],parameter->status, parameter->minInterval*1.25, parameter->maxInterval*1.25, parameter->latency, parameter->connectionTimeout*10);
            
            //first time read
            if ([myPeripheral retrieveBackupConnectionParameter]->status == 0xff) {
                [myPeripheral updateBackupConnectionParameter:parameter];
            }
            else {
                switch (myPeripheral.updateConnectionParameterStep) {
                    case UPDATE_PARAMETERS_STEP_PREPARE:
                        if ((myPeripheral.proprietaryDelegate != nil) && ([(NSObject *)myPeripheral.proprietaryDelegate respondsToSelector:@selector(MyPeripheral:didUpdateConnectionParameterAllowStatus:)]))
                            [myPeripheral.proprietaryDelegate MyPeripheral:myPeripheral didUpdateConnectionParameterAllowStatus:(buf[0] == 0x00)];
                            break;
                    case UPDATE_PARAMETERS_STEP_CHECK_RESULT:
                        if (buf[0] != 0x00) {
                            logInfo(@"[CBController] check connection parameter status again");
                            [myPeripheral checkConnectionParameterStatus];
                        }
                        else {
                            if ((myPeripheral.proprietaryDelegate != nil) && ([(NSObject *)myPeripheral.proprietaryDelegate respondsToSelector:@selector(MyPeripheral:didUpdateConnectionParameterStatus:interval:timeout:latency:)])){
                                if ([myPeripheral compareBackupConnectionParameter:parameter] == TRUE) {
                                    logInfo(@"[CBController] connection parameter no change");
                                    [myPeripheral.proprietaryDelegate MyPeripheral:myPeripheral didUpdateConnectionParameterStatus:FALSE interval:parameter->maxInterval*1.25 timeout:parameter->connectionTimeout*10 latency:parameter->latency];
                                }
                                else {
                                    //logInfo(@"connection parameter update success");
                                    [myPeripheral.proprietaryDelegate MyPeripheral:myPeripheral didUpdateConnectionParameterStatus:TRUE interval:parameter->maxInterval*1.25 timeout:parameter->connectionTimeout*10 latency:parameter->latency];
                                    [myPeripheral updateBackupConnectionParameter:parameter];
                                }
                            }
                        }
                    default:
                        break;
                }
           }
        }
        else if ([characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_AIR_PATCH_CHAR]]) {
            [myPeripheral updateAirPatchEvent:characteristic.value];
        }
        else if ((_transServiceUUID == nil) && [characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_ISSC_TRANS_TX]]) {
            if ((myPeripheral.transDataDelegate != nil) && ([(NSObject *)myPeripheral.transDataDelegate respondsToSelector:@selector(MyPeripheral:didReceiveTransparentData:)])) {
                [myPeripheral.transDataDelegate MyPeripheral:myPeripheral didReceiveTransparentData:characteristic.value];
            }
        }
    }
    else if (_transServiceUUID && [characteristic.service.UUID isEqual:_transServiceUUID]) {
        if ([characteristic.UUID isEqual:_transTxUUID]) {
            if ((myPeripheral.transDataDelegate != nil) && ([(NSObject *)myPeripheral.transDataDelegate respondsToSelector:@selector(MyPeripheral:didReceiveTransparentData:)])) {
                [myPeripheral.transDataDelegate MyPeripheral:myPeripheral didReceiveTransparentData:characteristic.value];
            }
        }
    }
}

- (void) peripheral:(CBPeripheral *)aPeripheral didWriteValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error 
{
//    logInfo(@"[CBController] didWriteValueForCharacteristic error msg:%d, %@, %@", error.code ,[error localizedFailureReason], [error localizedDescription]);
//    logInfo(@"characteristic data = %@ id = %@",characteristic.value,characteristic.UUID);
    MyPeripheral *myPeripheral = [self retrieveMyPeripheral:aPeripheral];
    if (myPeripheral == nil) {
        return;
    }
    if ([myPeripheral.transmit isReliableBurstTransmit:characteristic]) {
        return;
    }
    if ((_transServiceUUID == nil) && [characteristic.UUID isEqual:[CBUUID UUIDWithString:UUIDSTR_ISSC_TRANS_RX]]) {
        if ((myPeripheral.transDataDelegate != nil) && ([(NSObject *)myPeripheral.transDataDelegate respondsToSelector:@selector(MyPeripheral:didSendTransparentDataStatus:)])) {
            [myPeripheral.transDataDelegate MyPeripheral:myPeripheral didSendTransparentDataStatus:error];
        }
    }
    else if (_transServiceUUID && [characteristic.UUID isEqual:_transRxUUID]) {
        if ((myPeripheral.transDataDelegate != nil) && ([(NSObject *)myPeripheral.transDataDelegate respondsToSelector:@selector(MyPeripheral:didSendTransparentDataStatus:)])) {
            [myPeripheral.transDataDelegate MyPeripheral:myPeripheral didSendTransparentDataStatus:error];
        }
    }
}

- (void) peripheral:(CBPeripheral *)aPeripheral didDiscoverDescriptorsForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
{
    logInfo(@"[CBController] didDiscoverDescriptorsForCharacteristic error msg:%d, %@, %@", error.code ,[error localizedFailureReason], [error localizedDescription]);
}

- (void) peripheral:(CBPeripheral *)peripheral didUpdateValueForDescriptor:(CBDescriptor *)descriptor error:(NSError *)error
{
    logInfo(@"[CBController] didUpdateValueForDescriptor");
}

-(void) peripheral:(CBPeripheral *)peripheral didUpdateNotificationStateForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
{
    logInfo(@"[CBController] didUpdateNotificationStateForCharacteristic, UUID = %@", characteristic.UUID);
    MyPeripheral *myPeripheral = [self retrieveMyPeripheral:peripheral];
    if (myPeripheral == nil) {
        return;
    }
    if ((myPeripheral.transDataDelegate != nil) && ([(NSObject *)myPeripheral.transDataDelegate respondsToSelector:@selector(MyPeripheral:didUpdateTransDataNotifyStatus:)])) {
        [myPeripheral.transDataDelegate MyPeripheral:myPeripheral didUpdateTransDataNotifyStatus:characteristic.isNotifying];
        myPeripheral.isNotifying = characteristic.isNotifying;
    }
    
}

@end
