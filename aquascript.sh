nmap -sn 10.10.22.0/24 -oG - | awk '/Up$/{print $2}' > aqua_hosts.txt
nmap -T5 -iL aqua_hosts.txt -Pn -p 0-501,503-65535 -oX scan.xml
read -p "Are you ready to upload the results?"
wget https://github.com/michenriksen/aquatone/releases/download/v1.7.0/aquatone_linux_amd64_1.7.0.zip -O aquatone.zip
mkdir aqua
unzip aquatone.zip
cat scan.xml | ./aquatone -out aqua -nmap
cd aqua
zip -r ../aquatonereport.zip *
cd ..
curl -i -X POST -H "Content-Type: multipart/form-data" -F 'data=@aquatonereport.zip' http://localhost:7777/upload
