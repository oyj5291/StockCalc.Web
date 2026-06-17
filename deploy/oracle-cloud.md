# Oracle Cloud 배포 가이드

이 문서는 `주식계산연구소`를 Oracle Cloud Ubuntu VM에 배포하기 위한 준비 절차입니다.

## 1. 기본 구조

권장 구조:

```text
GitHub 또는 Git 원격 저장소
  -> Oracle Cloud Ubuntu VM
  -> /var/www/stockcalc-web 에 게시 파일 배치
  -> systemd 서비스로 앱 실행
  -> Nginx가 80/443 요청을 앱의 127.0.0.1:5000으로 전달
```

## 2. Oracle Cloud에서 준비할 것

1. Ubuntu 24.04 VM 생성
2. 공인 IP 연결
3. 보안 목록 또는 네트워크 보안 그룹에서 포트 허용
   - SSH: 22
   - HTTP: 80
   - HTTPS: 443
4. 도메인을 사용할 경우 DNS A 레코드를 VM 공인 IP로 연결

## 3. 서버 패키지 설치

Ubuntu 24.04 기준:

```bash
sudo apt-get update
sudo apt-get install -y nginx git
sudo apt-get install -y aspnetcore-runtime-10.0
```

서버에서 직접 빌드할 계획이면 SDK도 설치합니다.

```bash
sudo apt-get install -y dotnet-sdk-10.0
```

Microsoft 공식 문서 기준으로 Ubuntu 24.04는 `aspnetcore-runtime-10.0`과 `dotnet-sdk-10.0` 설치를 지원합니다.

## 4. 앱 게시

로컬 PC에서 게시 파일을 만들 경우:

```powershell
cd C:\Users\optro\source\repos\StockCalc.Web
dotnet publish .\StockCalc.Web\StockCalc.Web.csproj -c Release -o .\publish
```

게시 파일을 서버로 복사합니다.

```powershell
scp -r .\publish\* ubuntu@서버IP:/tmp/stockcalc-web/
```

서버에서 배치합니다.

```bash
sudo mkdir -p /var/www/stockcalc-web
sudo rsync -av --delete /tmp/stockcalc-web/ /var/www/stockcalc-web/
sudo chown -R www-data:www-data /var/www/stockcalc-web
```

## 5. systemd 서비스 등록

이 저장소의 샘플 파일을 서버에 복사합니다.

```bash
sudo cp deploy/stockcalc-web.service /etc/systemd/system/stockcalc-web.service
sudo systemctl daemon-reload
sudo systemctl enable stockcalc-web
sudo systemctl start stockcalc-web
sudo systemctl status stockcalc-web
```

로그 확인:

```bash
journalctl -u stockcalc-web -f
```

## 6. Nginx 설정

도메인을 사용할 경우 `deploy/nginx-stockcalc.conf`의 `server_name`을 실제 도메인으로 바꿉니다.

```bash
sudo cp deploy/nginx-stockcalc.conf /etc/nginx/sites-available/stockcalc-web
sudo ln -s /etc/nginx/sites-available/stockcalc-web /etc/nginx/sites-enabled/stockcalc-web
sudo nginx -t
sudo systemctl reload nginx
```

## 7. HTTPS 적용

도메인 연결 후 Certbot을 사용할 수 있습니다.

```bash
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d example.com -d www.example.com
```

`example.com`은 실제 도메인으로 바꿉니다.

## 8. 업데이트 배포

새 버전을 게시한 뒤 서버에 복사하고 서비스를 재시작합니다.

```bash
sudo rsync -av --delete /tmp/stockcalc-web/ /var/www/stockcalc-web/
sudo chown -R www-data:www-data /var/www/stockcalc-web
sudo systemctl restart stockcalc-web
sudo systemctl status stockcalc-web
```

## 참고 문서

- .NET Ubuntu 설치: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu
- ASP.NET Core + Nginx 배포: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx
