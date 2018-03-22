@echo off
echo create WeiboHufenService
sc create WeiboHufenService binPath= "F:\Project\WeiboHufen\WeiboHufen.Job\bin\Release\WeiboHufen.Job.exe"

echo waiting ...
choice /t 5 /d y /n >nul 

sc config WeiboHufenService start = AUTO 

echo waiting ...
choice /t 5 /d y /n >nul 
net start WeiboHufenService