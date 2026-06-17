# GitHub 및 Oracle Cloud Ubuntu 배포 가이드

이 문서는 `MoneyCalc.Web` 배포 절차를 기준으로 `StockCalc.Web`에 맞게 정리한 배포 체크리스트입니다.

## 0. 현재 프로젝트 기준값

| 항목 | 값 |
| --- | --- |
| GitHub 저장소 | `https://github.com/oyj5291/StockCalc.Web` |
| 기본 브랜치 | `main` |
| 도메인 | `stockcalc.ai.kr` |
| 서버 IP | `168.107.22.55` |
| OS | Ubuntu 24.04 LTS |
| 앱 이름 | `stockcalc` |
| systemd 서비스 | `stockcalc.service` |
| 소스 경로 | `/opt/stockcalc-src` |
| 배포 경로 | `/var/www/stockcalc` |
| ASP.NET 포트 | `127.0.0.1:5001` |
| Nginx 포트 | `80`, `443` |

`MoneyCalc.Web`과 같은 Oracle VM에 함께 올리는 구성입니다.

```text
moneycalc.ai.kr -> Nginx -> 127.0.0.1:5000 -> MoneyCalc.Web
stockcalc.ai.kr -> Nginx -> 127.0.0.1:5001 -> StockCalc.Web
```

## 1. 로컬 Git 준비

프로젝트 루트에서 현재 상태를 확인합니다.

```powershell
git status
```

변경사항이 있으면 커밋하고 GitHub에 올립니다.

```powershell
git add .
git commit -m "변경 내용 요약"
git push origin main
```

현재 원격 저장소:

```powershell
git remote -v
```

기대값:

```text
origin  https://github.com/oyj5291/StockCalc.Web.git
```

## 2. Oracle Cloud 서버 준비

`MoneyCalc.Web`이 이미 같은 VM에서 동작 중이면 VM 생성, Nginx 설치, Certbot 설치, 방화벽 설정은 대부분 완료되어 있을 가능성이 큽니다.

새 서버라면 Oracle Cloud에서 Ubuntu 24.04 VM을 생성합니다.

권장 최소 사양:

```text
OS: Ubuntu 24.04 LTS
CPU: 1 Core 이상
Memory: 1GB 이상
Boot Volume: 40GB 이상
```

보안 목록 또는 NSG Ingress Rule:

| 포트 | 프로토콜 | 소스 | 용도 |
| --- | --- | --- | --- |
| 22 | TCP | `0.0.0.0/0` 또는 내 IP | SSH |
| 80 | TCP | `0.0.0.0/0` | HTTP |
| 443 | TCP | `0.0.0.0/0` | HTTPS |

내부 앱 포트 `5000`, `5001`은 외부에 열지 않습니다.

Windows PowerShell에서 접속합니다.

```powershell
ssh -i C:\path\oracle.key ubuntu@168.107.22.55
```

## 3. .NET SDK / Runtime 설치

서버에서 Git으로 소스를 받고 직접 `dotnet publish`를 실행하려면 SDK가 필요합니다.

설치 여부 확인:

```bash
dotnet --list-sdks
dotnet --list-runtimes
```

Microsoft 패키지 저장소가 아직 없다면 등록합니다.

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
```

Runtime과 SDK를 설치합니다.

```bash
sudo apt install -y aspnetcore-runtime-10.0 dotnet-sdk-10.0
```

## 4. Git, Nginx, Certbot 확인

```bash
git --version
nginx -v
certbot --version
```

없으면 설치합니다.

```bash
sudo apt install -y git nginx certbot python3-certbot-nginx
```

Ubuntu 방화벽을 사용할 경우:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw status
```

주의: Oracle Cloud 보안 목록과 Ubuntu UFW가 모두 열려 있어야 외부 접속이 됩니다.

## 5. 서버에서 Git clone

최초 배포:

```bash
sudo mkdir -p /opt/stockcalc-src
sudo chown -R ubuntu:ubuntu /opt/stockcalc-src
git clone https://github.com/oyj5291/StockCalc.Web.git /opt/stockcalc-src
```

이미 clone 되어 있으면:

```bash
cd /opt/stockcalc-src
git pull origin main
```

## 6. 서버에서 게시 파일 생성

```bash
cd /opt/stockcalc-src
dotnet publish ./StockCalc.Web/StockCalc.Web.csproj -c Release -o ./artifacts/publish
```

게시 결과:

```text
/opt/stockcalc-src/artifacts/publish
```

## 7. systemd 서비스 등록

앱 폴더를 만들고 게시 파일을 복사합니다.

```bash
sudo mkdir -p /var/www/stockcalc
sudo rsync -av --delete /opt/stockcalc-src/artifacts/publish/ /var/www/stockcalc/
sudo chown -R www-data:www-data /var/www/stockcalc
sudo chmod -R u=rwX,g=rX,o= /var/www/stockcalc
```

서비스 파일을 등록합니다.

```bash
sudo cp /opt/stockcalc-src/deploy/stockcalc.service /etc/systemd/system/stockcalc.service
sudo systemctl daemon-reload
sudo systemctl enable --now stockcalc
sudo systemctl status stockcalc --no-pager
```

서비스 파일 핵심값:

```ini
WorkingDirectory=/var/www/stockcalc
ExecStart=/usr/bin/dotnet /var/www/stockcalc/StockCalc.Web.dll
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5001
```

로컬 앱 확인:

```bash
curl --fail http://127.0.0.1:5001/healthz
```

로그 확인:

```bash
sudo journalctl -u stockcalc -n 100 --no-pager
sudo journalctl -u stockcalc -f
```

## 8. Nginx 리버스 프록시 설정

`MoneyCalc.Web`의 Nginx 설정은 그대로 두고, `StockCalc.Web`용 사이트 설정을 하나 더 추가합니다.

```bash
sudo cp /opt/stockcalc-src/deploy/nginx-stockcalc.conf /etc/nginx/sites-available/stockcalc
sudo ln -s /etc/nginx/sites-available/stockcalc /etc/nginx/sites-enabled/stockcalc
sudo nginx -t
sudo systemctl reload nginx
```

이미 `MoneyCalc.Web`에서 default 사이트를 지웠다면 다시 지울 필요 없습니다. 아직 남아 있으면 한 번만 지웁니다.

```bash
sudo rm -f /etc/nginx/sites-enabled/default
```

Nginx 설정 핵심값:

```nginx
server_name stockcalc.ai.kr www.stockcalc.ai.kr;
proxy_pass http://127.0.0.1:5001;
```

HTTP 접속 확인:

```bash
curl -I http://stockcalc.ai.kr
curl --fail http://stockcalc.ai.kr/healthz
```

## 9. DNS 연결

도메인 관리 사이트에서 A 레코드를 등록합니다.

| 이름 | 타입 | 값 |
| --- | --- | --- |
| `@` | A | `168.107.22.55` |
| `www` | A | `168.107.22.55` |

DNS 반영 확인:

```powershell
nslookup stockcalc.ai.kr
nslookup www.stockcalc.ai.kr
```

브라우저에서 `http://stockcalc.ai.kr` 접속이 되어야 HTTPS 발급을 진행할 수 있습니다.

## 10. HTTPS 적용

DNS가 서버 IP로 정상 연결되고 HTTP 접속이 확인된 뒤 실행합니다.

```bash
sudo certbot --nginx -d stockcalc.ai.kr -d www.stockcalc.ai.kr
```

인증서 자동 갱신 테스트:

```bash
sudo certbot renew --dry-run
```

최종 확인:

```bash
curl --fail https://stockcalc.ai.kr/healthz
```

## 11. Git 기반 이후 버전 업데이트

서버에서 다음만 실행하면 됩니다.

```bash
cd /opt/stockcalc-src
git pull origin main
dotnet publish ./StockCalc.Web/StockCalc.Web.csproj -c Release -o ./artifacts/publish
sudo systemctl stop stockcalc
sudo rsync -av --delete ./artifacts/publish/ /var/www/stockcalc/
sudo chown -R www-data:www-data /var/www/stockcalc
sudo systemctl start stockcalc
sudo systemctl status stockcalc --no-pager
curl --fail http://127.0.0.1:5001/healthz
```

저장소에 포함된 스크립트로 줄여서 실행할 수도 있습니다.

```bash
cd /opt/stockcalc-src
chmod +x ./deploy/deploy-from-git.sh
./deploy/deploy-from-git.sh
```

## 12. 장애 확인 순서

브라우저에서 타임아웃:

```text
1. Oracle Cloud 보안 목록에서 80, 443이 열려 있는지 확인
2. Ubuntu UFW에서 Nginx Full이 허용되어 있는지 확인
3. Nginx가 실행 중인지 확인
```

확인 명령:

```bash
sudo ufw status
sudo systemctl status nginx --no-pager
sudo ss -tlnp
```

502 Bad Gateway:

```text
Nginx는 살아 있지만 StockCalc.Web 앱이 죽었거나 포트가 다릅니다.
```

확인 명령:

```bash
sudo systemctl status stockcalc --no-pager
sudo journalctl -u stockcalc -n 100 --no-pager
curl -I http://127.0.0.1:5001
```

HTTPS 인증서 실패:

```text
1. 도메인 A 레코드가 서버 IP를 가리키는지 확인
2. HTTP 접속이 먼저 되는지 확인
3. Nginx server_name이 도메인과 일치하는지 확인
```

확인 명령:

```bash
sudo nginx -t
sudo certbot certificates
sudo tail -n 100 /var/log/nginx/error.log
```

## 13. 배포 후 SEO 확인

현재 `StockCalc.Web`은 `/healthz`만 준비되어 있습니다. `robots.txt`와 `sitemap.xml`은 추후 SEO 작업 때 추가합니다.

배포가 끝나면 아래 URL을 확인합니다.

```text
https://stockcalc.ai.kr/
https://stockcalc.ai.kr/healthz
```

## 14. 빠른 체크리스트

```text
[ ] GitHub 저장소 push 완료
[ ] Oracle Cloud 보안 목록 22, 80, 443 개방
[ ] SSH 접속 확인
[ ] .NET SDK 10 설치
[ ] Nginx / Certbot 설치 확인
[ ] /opt/stockcalc-src clone
[ ] dotnet publish 성공
[ ] /var/www/stockcalc 배치
[ ] stockcalc.service 등록
[ ] curl localhost:5001/healthz 확인
[ ] nginx-stockcalc.conf 등록
[ ] DNS A 레코드 연결
[ ] HTTP 접속 확인
[ ] Certbot HTTPS 적용
[ ] HTTPS 접속 확인
```
