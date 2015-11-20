-- 데이터베이스 생성
CREATE DATABASE vampdb charset=utf8;
-- 사용자 아이디 생성 및 권한 부여
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, DROP, ALTER
	ON vampdb.* TO 'ilumia'@'localhost'
    IDENTIFIED BY 'vjpp56';
-- 인코딩 확인 (목표 인코딩: utf-8)
SELECT schema_name, default_character_set_name 
FROM information_schema.SCHEMATA;

ALTER DATABASE vampdb charset=utf8;

-- TestTable 테이블 생성
CREATE TABLE users (
  userid      VARCHAR(30) PRIMARY KEY,
  userpassword    VARCHAR(60) not null,
  cardset VARCHAR(800)
);
CREATE TABLE friends (
	userid VARCHAR(30),
	friendid VARCHAR(30),
	primary key (userid, friendid),
	foreign key (userid) references users(userid) ON DELETE CASCADE ON UPDATE CASCADE,
	foreign key (friendid) references users(userid) ON DELETE CASCADE ON UPDATE CASCADE
);

DROP TABLE users;
DROP TABLE friends;

select * from users;
select * from friends;
-- cardset: cardno
INSERT INTO users VALUES('InitID', 'InitPassword', NULL);
INSERT INTO friends VALUES('InitID', 'InitID');
DELETE FROM users;
DELETE FROM friends;
