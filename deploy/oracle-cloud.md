# Oracle Cloud Ubuntu 배포

이 문서는 `stockcalc.ai.kr` 기준으로 작성했습니다. 다른 도메인을 사용할 경우 `nginx-stockcalc.conf`의 `server_name`을 먼저 변경합니다.

## 1. 로컬에서 게시

솔루션 루트에서 실행합니다.

```powershell
dotnet publish .\StockCalc.Web\StockCalc.Web.csproj -c Release -o .\artifacts\publish
```

게시 결과와 배포 설정 파일을 서버로 전송합니다. 아래의 키 경로, 서버 IP는 실제 값으로 바꿉니다.

```powershell
scp -i C:\path\oracle.key -r .\artifacts\publish\* ubuntu@SERVER_IP:/tmp/stockcalc/
scp -i C:\path\oracle.key .\deploy\stockcalc.service ubuntu@SERVER_IP:/tmp/
scp -i C:\path\oracle.key .\deploy\nginx-stockcalc.conf ubuntu@SERVER_IP:/tmp/
```

## 2. Ubuntu 패키지 설치

Ubuntu 24.04 이상에서는 다음 패키지를 설치합니다.

```bash
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-10.0 nginx certbot python3-certbot-nginx
dotnet --list-runtimes
```

`aspnetcore-runtime-10.0`을 찾지 못하면 backports 저장소를 추가한 뒤 다시 설치합니다.

```bash
sudo add-apt-repository ppa:dotnet/backports
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-10.0
```

## 3. 애플리케이션 설치

```bash
sudo mkdir -p /var/www/stockcalc
sudo cp -a /tmp/stockcalc/. /var/www/stockcalc/
sudo chown -R www-data:www-data /var/www/stockcalc
sudo chmod -R u=rwX,g=rX,o= /var/www/stockcalc

sudo cp /tmp/stockcalc.service /etc/systemd/system/stockcalc.service
sudo systemctl daemon-reload
sudo systemctl enable --now stockcalc
sudo systemctl status stockcalc --no-pager
curl --fail http://127.0.0.1:5000/healthz
```

로그 확인:

```bash
sudo journalctl -u stockcalc -n 100 --no-pager
sudo journalctl -u stockcalc -f
```

## 4. Nginx 연결

```bash
sudo cp /tmp/nginx-stockcalc.conf /etc/nginx/sites-available/stockcalc
sudo ln -s /etc/nginx/sites-available/stockcalc /etc/nginx/sites-enabled/stockcalc
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

Oracle Cloud 보안 목록과 Ubuntu 방화벽에서 TCP 80, 443 포트를 허용합니다.

```bash
sudo ufw allow 'Nginx Full'
sudo ufw status
```

DNS에서 `stockcalc.ai.kr`과 `www.stockcalc.ai.kr`의 A 레코드를 Oracle VM 공인 IP로 연결한 뒤 HTTP 접속을 확인합니다.

## 5. HTTPS 적용

DNS 전파와 HTTP 접속 확인 후 실행합니다.

```bash
sudo certbot --nginx -d stockcalc.ai.kr -d www.stockcalc.ai.kr
sudo certbot renew --dry-run
```

최종 확인:

```bash
curl --fail https://stockcalc.ai.kr/healthz
```

## 6. 이후 버전 업데이트

새 게시 파일을 `/tmp/stockcalc`로 전송한 뒤 다음 순서로 교체합니다.

```bash
sudo systemctl stop stockcalc
sudo cp -a /tmp/stockcalc/. /var/www/stockcalc/
sudo chown -R www-data:www-data /var/www/stockcalc
sudo systemctl start stockcalc
curl --fail http://127.0.0.1:5000/healthz
```

배포 실패 시 먼저 `systemctl status stockcalc`와 `journalctl -u stockcalc`를 확인합니다.
