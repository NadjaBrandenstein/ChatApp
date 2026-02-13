DROP SCHEMA IF EXISTS chatapp CASCADE;
CREATE SCHEMA IF NOT EXISTS chatapp;

-- ===========================
-- Table: Role
-- ===========================
CREATE TABLE chatapp.Role (
    RoleID SERIAL PRIMARY KEY,
    RoleName VARCHAR(100) NOT NULL
);

-- Seed roles 
INSERT INTO chatapp.Role (RoleName)
VALUES ('User'), ('Admin');

-- ===========================
-- Table: Login
-- ===========================
CREATE TABLE chatapp.Login (
    UserID SERIAL PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    RoleID INT NOT NULL,
    CONSTRAINT fk_login_role
       FOREIGN KEY (RoleID)
           REFERENCES chatapp.Role (RoleID)
           ON DELETE CASCADE
);

-- ===========================
-- Table: Chat Room
-- ===========================
CREATE TABLE chatapp.Room (
    RoomID SERIAL PRIMARY KEY,
    RoomName VARCHAR(100) NOT NULL
);

-- ===========================
-- Table: Message
-- ===========================
CREATE TABLE chatapp.Message(
    MessageID SERIAL PRIMARY KEY,
    content TEXT NOT NULL,
    SentAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    SenderUserID INT NOT NULL,
    RoomID INT NOT NULL,
    
    -- NULL = public message
    -- NOT NULL = DM (admin only, enforced in app logic)
    RecipientUserID INT NULL,
    
    CONSTRAINT fk_message_sender
        FOREIGN KEY (SenderUserID)
            REFERENCES chatapp.Login (UserID)
            ON DELETE CASCADE,
    
    CONSTRAINT fk_message_room
        FOREIGN KEY (RoomID)
            REFERENCES chatapp.Room (RoomID)
            ON DELETE CASCADE,
    
    CONSTRAINT fk_message_recipient
        FOREIGN KEY (RecipientUserID)
            REFERENCES chatapp.Login (UserID)
            ON DELETE CASCADE
);



