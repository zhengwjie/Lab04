int PWMPin;
int temp=0;
int light=1;
byte mid[3];
byte pwm[3];
void setup() {
  // put your setup code here, to run once:
Serial.begin(38400);
}
void PrintMid(int Pin)
{
  int k=analogRead(Pin);
  mid[0]=(0xE<<4)+Pin;
  //Serial.print(mid[0]);
  mid[1]=(byte)k&0x7f;
  mid[2]=(byte)((k>>7)&0x7f);
  int a=(int)mid[1]+((int)mid[2]<<7);
  Serial.write(mid,3);
  delay(1000);
}
void ControlPWM()
{
  while(Serial.available()>0)
  {
    pwm[0]=Serial.read();
    if(pwm[0]>>4==0xD)
    {
        pwm[1]=Serial.read();
        pwm[2]=Serial.read();
        PWMPin=pwm[0]&0xf;
        int value=(pwm[1]+(pwm[2]<<7));
        analogWrite(PWMPin,value);
    }
  }
}
void loop() {
  // put your main code here, to run repeatedly:
PrintMid(0);
PrintMid(1);
ControlPWM();
delay(1000);
}
