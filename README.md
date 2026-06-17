# 주식계산연구소

주식 투자자를 위한 계산기와 투자 기초 설명 사이트입니다.

## 기술 구성

- C# / .NET 10
- ASP.NET Core MVC
- Razor Views
- 서버 저장소나 DB 없음

## 주요 기능

- 주식 수익률 계산기
- 평단가 계산기
- 물타기 계산기
- 손절가 계산기
- 목표가 계산기
- 배당금 계산기
- 복리 수익률 계산기
- 투자 기초 글

## 로컬 실행

```powershell
cd C:\Users\optro\source\repos\StockCalc.Web\StockCalc.Web
dotnet run
```

브라우저에서 `http://localhost:5271` 또는 터미널에 표시되는 URL로 접속합니다.

## 빌드

```powershell
dotnet build .\StockCalc.Web\StockCalc.Web.csproj
```

## 배포 준비

Oracle Cloud Ubuntu VM 배포 절차는 [deploy/oracle-cloud.md](deploy/oracle-cloud.md)를 참고하세요.

## Git 원격 저장소

`MoneyCalc.Web`과 같은 GitHub 계정/패턴을 사용한다면 다음 주소를 원격 저장소로 사용합니다.

```powershell
git remote add origin https://github.com/oyj5291/StockCalc.Web.git
git push -u origin main
```
