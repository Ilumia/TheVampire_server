-- 데이터베이스 생성
CREATE DATABASE vampdb;
-- 사용자 아이디 생성 및 권한 부여
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, ALTER
	ON vampdb.* TO 'ilumia'@'localhost'
    IDENTIFIED BY 'vjpp56';
-- 인코딩 확인
SELECT schema_name, default_character_set_name 
FROM information_schema.SCHEMATA;

-- TestTable 테이블 생성
CREATE TABLE users (
  userid      VARCHAR(30) PRIMARY KEY,
  userpassword    VARCHAR(60) not null,
  socialid VARCHAR(30)
);
CREATE TABLE friends (
	userid VARCHAR(30),
	friendid VARCHAR(30),
	primary key (userid, friendid),
	foreign key (userid) references users(userid),
	foreign key (friendid) references users(userid)
);

DROP TABLE users;
DROP TABLE friends;

select * from users;
select * from friends;

INSERT INTO users VALUES('InitID', 'InitPassword', NULL);
INSERT INTO users VALUES('InitID2', 'InitPassword', NULL);
DELETE FROM users;
INSERT INTO friends VALUES('InitID', 'InitID');
INSERT INTO friends VALUES('InitID', 'InitID2');
DELETE FROM friends;