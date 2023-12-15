## 블록체인 토이프로젝트
### 21.03.02 ~
#### 블록체인 구조 이해를 위함

--------------------------------------------------------------------

### 로컬에서 실행
#### 1. 원하는 위치에 클론
```sh
$ git clone https://github.com/yoonmyung/ToyBlockChain.git
```
#### 2-1. 해당 레파지토리 폴더로 이동 후 아래의 genesis block 다운로드 하여 압축 해제
[_Genesisblock.zip](https://github.com/yoonmyung/ToyBlockChain/files/6527240/_Genesisblock.zip)

#### 2-2. 아래의 실행파일 다운로드 하여 압축 해제
[netcoreapp3.1.zip](https://github.com/yoonmyung/ToyBlockChain/files/6527261/netcoreapp3.1.zip)

※2번 과정은 추후 간략화

#### 3. 프로젝트 실행
```sh
$ 경로\BlockChain\netcoreapp3.1\PracticeBlockChain.Test.exe [노드 유형] [원하는 포트 번호] [private key]
```
아래의 순서대로 3개의 node를 실행
1. seed node 실행

새로운 node 접속 시 라우팅 테이블 전송. 포트 번호가 65000으로 고정돼있어 별다른 옵션이 필요 없음
```sh
seed
```
2. miner node 실행 (player node를 먼저 실행해도 무방)

일정 시간마다 빈 블록 생성. player node가 수행한 게임 작업이 있을 경우, 이를 블록의 액션에 넣어 채운 블록을 생성.
```sh
miner [원하는 포트 번호] [private key]
```
3. player node 실행

실질적으로 게임을 플레이하는 주체. 추후 게임 플레이 추가
```sh
player [원하는 포트 번호] [private key]
```

※miner, player의 경우 private key 생략시 새로운 노드로 간주하여 생성함

※기존 노드로 플레이를 원할 경우 private key는 레파지토리 폴더 내 `_Player` 폴더에 있는 private key를 그대로 복사해서 붙여넣기


#### Example
```sh
$ 경로\BlockChain\PracticeBlockChain.Test\bin\Release\netcoreapp3.1\PracticeBlockChain.Test.exe miner 65002 1-2-3-4-5-6-7-8-9-10-11-12-13-14-15-16-17-18-19-20-21-22-23-24-25-26-27-28-29-30-31-32
```
1. seed 실행
![image](https://user-images.githubusercontent.com/40621689/119242613-51671580-bb9a-11eb-934f-ae31992761ca.png)
2. miner 실행
![image](https://user-images.githubusercontent.com/40621689/119242597-1c5ac300-bb9a-11eb-9eda-e0503ef83949.png)
3. player 실행
![image](https://user-images.githubusercontent.com/40621689/119242629-680d6c80-bb9a-11eb-9316-d34d0a91685a.png)

--------------------------------------------------------------------

### 개요
0. 이더리움 기반 블록체인 구조 + [libplanet](https://github.com/planetarium/libplanet/tree/main/Libplanet) 참고해서 이해
1. 로컬에서 돌아가는 블록체인 구조를 1차적으로 구현
2. P2P 기반 네트워크에서 돌아가는 블록체인 구조로 발전
3. 그위에서 체인 구조에 맞게 돌아가는 [틱텍토 게임](https://ko.wikipedia.org/wiki/%ED%8B%B1%ED%83%9D%ED%86%A0) 추가

--------------------------------------------------------------------

### 개발 과정
|날짜           |설명|커밋|
|:-------------:|:-----------------------------:|:-------------:|
|2021.03        | ||
|1주차          |블록체인, 블록 구조 구현 (체인에 빈 블록이 연결)|[관련 커밋](https://github.com/yoonmyung/ToyBlockChain/tree/55337aa7870fda7253fc2da7fe14a26ac99262c2)|
|2주차          | 빈 블록에 들어가는 트랜잭션(액션) 구조 구현|[관련 커밋](https://github.com/yoonmyung/ToyBlockChain/tree/8a2cfe2f0e9021c4a00027dbcbd75fbbf7fa66bc)|
|3주차          |(로컬)게임에서 발생하는 데이터를 액션에 넣는 구조 구현|[관련 커밋](https://github.com/yoonmyung/ToyBlockChain/tree/d7ce1caa40ed3019f24dbe4c98ba387137699516)|
|4주차          |블록체인을 파일 형태로 저장하여 게임을 다시 실행해도 이전 상태가 로딩될 수 있도록 함|[관련 커밋](https://github.com/yoonmyung/ToyBlockChain/tree/5e1887441502865f6297652d31d860f816c25d7e)|
|2021.04        | ||
|1주차          |게임에서 발생하는 플레이어 간 순서 제어 관련 버그 수정|[관련 커밋](https://github.com/yoonmyung/ToyBlockChain/tree/d12861ca65c5c614482c4c07cd002515706f5dc4)|
|2주차          |TCP기반 P2P 네트워크 구현|[관련 커밋](https://github.com/yoonmyung/ToyBlockChain/tree/658dce026fef191aab7b093834352b76fe6add04)|
|3~4주차        |네트워크에 블록체인 구조 연결|[관련 커밋](https://github.com/yoonmyung/ToyBlockChain/tree/de4ab07849b6d2767d3536e95aee48c527fc2565)|
|2021.05| ||
|추후           |(네트워크)게임을 블록체인 구조에 올릴 수 있게 수정||

--------------------------------------------------------------------

### 문서
이해한 내용을 기반으로 블록체인 개념 정리.. 업로드 예정!

