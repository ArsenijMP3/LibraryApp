-- ============================================
-- 1. СОЗДАНИЕ ТАБЛИЦ (без внешних ключей)
-- ============================================

CREATE TABLE Роли (
    id AUTOINCREMENT PRIMARY KEY,
    название_роли TEXT (50) NOT NULL UNIQUE
);

CREATE TABLE Читатели (
    id AUTOINCREMENT PRIMARY KEY,
    фио TEXT (150) NOT NULL,
    логин TEXT (50) NOT NULL UNIQUE,
    пароль TEXT (255) NOT NULL,
    роль_id LONG NOT NULL,
    контакт TEXT (100)
);

CREATE TABLE Книги (
    id AUTOINCREMENT PRIMARY KEY,
    название TEXT (200) NOT NULL,
    автор TEXT (100) NOT NULL,
    жанр TEXT (50) NOT NULL,
    год_издания SHORT,
    издательство TEXT (100),
    количество_экземпляров SHORT,
    описание MEMO,
    фото TEXT (255)
);

CREATE TABLE Выдачи (
    id AUTOINCREMENT PRIMARY KEY,
    книга_id LONG NOT NULL,
    читатель_id LONG NOT NULL,
    дата_выдачи DATETIME NOT NULL,
    дата_возврата DATETIME,
    статус TEXT (20) NOT NULL
);

-- ============================================
-- 2. ДОБАВЛЕНИЕ ВНЕШНИХ КЛЮЧЕЙ (ALTER TABLE)
-- ============================================

ALTER TABLE Читатели
ADD CONSTRAINT fk_Читатели_Роли FOREIGN KEY (роль_id) REFERENCES Роли (id);

ALTER TABLE Выдачи
ADD CONSTRAINT fk_Выдачи_Книги FOREIGN KEY (книга_id) REFERENCES Книги (id);

ALTER TABLE Выдачи
ADD CONSTRAINT fk_Выдачи_Читатели FOREIGN KEY (читатель_id) REFERENCES Читатели (id);