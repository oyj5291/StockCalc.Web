# Oracle Cloud Ubuntu 배포

`MoneyCalc.Web`과 같은 Oracle Cloud VM에 함께 올리는 구성을 기준으로 작성했습니다.

## 서버 구성

같은 VM에서 두 사이트를 함께 운영할 수 있습니다. 핵심은 내부 포트를 다르게 쓰고, Nginx가 도메인별로 다른 앱에 연결하게 하는 것입니다.

```text
moneycalc.ai.kr  -> Nginx -> 127.0.0.1:5000 -> MoneyCalc.Web
stockcalc.ai.kr  -> Nginx -> 127.0.0.1:5001 -> StockCalc.Web
```

`MoneyCalc.Web`이 이미 `5000` 포트를 사용하므로 `StockCalc.Web`은 `5001` 포트를 사용합니다.

## 1. Git 기반 배포 방식

서버에서 GitHub 저장소를 직접 내려받아 빌드/게시하는 방식입니다. `MoneyCalc.Web`과 같은 서버에서 관리하기 쉽습니다.

서버 작업 디렉터리:

```text
/opt/stockcalc-src       Git 소스
/var/www/stockcalc       실행 파일 배치
/etc/systemd/system      systemd 서비스
/etc/nginx/sites-*       Nginx 사이트 설정
```

## 2. Ubuntu 패키지 확인

서버에서 직접 `dotnet publish`를 실행해야 하므로 runtime뿐 아니라 SDK도 필요합니다.

```bash
dotnet --list-sdks
dotnet --list-runtimes
nginx -v
```

처음 설치하는 서버라면 다음을 실행합니다.

```bash
sudo apt-get update
sudo apt-get install -y git nginx certbot python3-certbot-nginx aspnetcore-runtime-10.0 dotnet-sdk-10.0
```

`aspnetcore-runtime-10.0` 또는 `dotnet-sdk-10.0`을 찾지 못하면 backports 저장소를 추가한 뒤 다시 설치합니다.

```bash
sudo add-apt-repository ppa:dotnet/backports
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-10.0 dotnet-sdk-10.0
```

## 3. 최초 소스 내려받기

```bash
sudo mkdir -p /opt/stockcalc-src
sudo chown -R ubuntu:ubuntu /opt/stockcalc-src
git clone https://github.com/oyj5291/StockCalc.Web.git /opt/stockcalc-src
```

이미 폴더가 있다면:

```bash
cd /opt/stockcalc-src
git pull origin main
```

## 4. 게시 및 설치

```bash
cd /opt/stockcalc-src
dotnet publish ./StockCalc.Web/StockCalc.Web.csproj -c Release -o ./artifacts/publish

sudo mkdir -p /var/www/stockcalc
sudo rsync -av --delete ./artifacts/publish/ /var/www/stockcalc/
sudo chown -R www-data:www-data /var/www/stockcalc
sudo chmod -R u=rwX,g=rX,o= /var/www/stockcalc
```

## 5. systemd 서비스 등록

```bash
cd /opt/stockcalc-src
sudo cp ./deploy/stockcalc.service /etc/systemd/system/stockcalc.service
sudo systemctl daemon-reload
sudo systemctl enable --now stockcalc
sudo systemctl status stockcalc --no-pager
curl --fail http://127.0.0.1:5001/healthz
```

로그 확인:

```bash
sudo journalctl -u stockcalc -n 100 --no-pager
sudo journalctl -u stockcalc -f
```

## 6. Nginx 연결

`MoneyCalc.Web`의 Nginx 설정은 그대로 두고, `StockCalc.Web`용 사이트 설정을 하나 더 추가합니다.

```bash
cd /opt/stockcalc-src
sudo cp ./deploy/nginx-stockcalc.conf /etc/nginx/sites-available/stockcalc
sudo ln -s /etc/nginx/sites-available/stockcalc /etc/nginx/sites-enabled/stockcalc
sudo nginx -t
sudo systemctl reload nginx
```

이미 `MoneyCalc.Web`에서 default 사이트를 지웠다면 다시 지울 필요 없습니다. 아직 남아 있으면 한 번만 지웁니다.

```bash
sudo rm -f /etc/nginx/sites-enabled/default
```

Oracle Cloud 보안 목록과 Ubuntu 방화벽은 80, 443만 열면 됩니다. 내부 포트 `5000`, `5001`은 외부에 열지 않습니다.

```bash
sudo ufw allow 'Nginx Full'
sudo ufw status
```

DNS에서 `stockcalc.ai.kr`과 `www.stockcalc.ai.kr`의 A 레코드를 같은 Oracle VM 공인 IP로 연결합니다.

## 7. HTTPS 적용

DNS 전파와 HTTP 접속 확인 후 실행합니다.

```bash
sudo certbot --nginx -d stockcalc.ai.kr -d www.stockcalc.ai.kr
sudo certbot renew --dry-run
```

최종 확인:

```bash
curl --fail https://stockcalc.ai.kr/healthz
```

## 8. 이후 업데이트

서버에서 다음만 실행하면 됩니다.

```bash
cd /opt/stockcalc-src
git pull origin main
dotnet publish ./StockCalc.Web/StockCalc.Web.csproj -c Release -o ./artifacts/publish
sudo systemctl stop stockcalc
sudo rsync -av --delete ./artifacts/publish/ /var/www/stockcalc/
sudo chown -R www-data:www-data /var/www/stockcalc
sudo systemctl start stockcalc
curl --fail http://127.0.0.1:5001/healthz
```

위 과정을 줄이려면 저장소에 포함된 스크립트를 사용합니다.

```bash
cd /opt/stockcalc-src
chmod +x ./deploy/deploy-from-git.sh
./deploy/deploy-from-git.sh
```

배포 실패 시 먼저 `systemctl status stockcalc`와 `journalctl -u stockcalc`를 확인합니다.
