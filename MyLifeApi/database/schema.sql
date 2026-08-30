CREATE DATABASE IF NOT EXISTS mylife

USE mylife;


CREATE TABLE users (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    name VARCHAR(120) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,

    avatar_url VARCHAR(500) NULL,
    birth_date DATE NULL,

    status ENUM(
        'active',
        'inactive',
        'blocked'
    ) NOT NULL DEFAULT 'active',

    last_login_at DATETIME NULL,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP
);


CREATE TABLE categories (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(80) NOT NULL,
    description VARCHAR(255) NULL,

    icon VARCHAR(50) NULL,
    color VARCHAR(20) NULL,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_categories_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_categories_user_name
        UNIQUE (user_id, name)
);


CREATE TABLE activities (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,
    category_id BIGINT UNSIGNED NULL,

    title VARCHAR(150) NOT NULL,
    description TEXT NULL,

    activity_date DATE NOT NULL,

    started_at DATETIME NULL,
    ended_at DATETIME NULL,

    duration_seconds INT UNSIGNED NULL,

    status ENUM(
        'planned',
        'in_progress',
        'completed',
        'cancelled'
    ) NOT NULL DEFAULT 'completed',

    source ENUM(
        'manual',
        'timer',
        'imported'
    ) NOT NULL DEFAULT 'manual',

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_activities_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_activities_category
        FOREIGN KEY (category_id)
        REFERENCES categories(id)
        ON DELETE SET NULL,

    CONSTRAINT chk_activity_dates
        CHECK (
            ended_at IS NULL
            OR started_at IS NULL
            OR ended_at >= started_at
        ),

    CONSTRAINT chk_activity_duration
        CHECK (
            duration_seconds IS NULL
            OR duration_seconds >= 0
        )
);


CREATE TABLE goals (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,

    title VARCHAR(150) NOT NULL,
    description TEXT NULL,

    goal_type ENUM(
        'duration',
        'quantity',
        'money',
        'boolean'
    ) NOT NULL,

    target_value DECIMAL(12,2) NULL,
    unit VARCHAR(30) NULL,

    start_date DATE NULL,
    target_date DATE NULL,

    status ENUM(
        'active',
        'completed',
        'paused',
        'cancelled'
    ) NOT NULL DEFAULT 'active',

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_goals_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_goal_dates
        CHECK (
            target_date IS NULL
            OR start_date IS NULL
            OR target_date >= start_date
        ),

    CONSTRAINT chk_goal_target
        CHECK (
            target_value IS NULL
            OR target_value >= 0
        )
);


CREATE TABLE activity_goals (
    activity_id BIGINT UNSIGNED NOT NULL,
    goal_id BIGINT UNSIGNED NOT NULL,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (
        activity_id,
        goal_id
    ),

    CONSTRAINT fk_activity_goals_activity
        FOREIGN KEY (activity_id)
        REFERENCES activities(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_activity_goals_goal
        FOREIGN KEY (goal_id)
        REFERENCES goals(id)
        ON DELETE CASCADE
);


CREATE TABLE accounts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(100) NOT NULL,

    account_type ENUM(
        'checking',
        'savings',
        'cash',
        'wallet',
        'investment',
        'other'
    ) NOT NULL,

    initial_balance DECIMAL(14,2) NOT NULL DEFAULT 0.00,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_accounts_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE
);


CREATE TABLE financial_categories (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,

    name VARCHAR(80) NOT NULL,

    category_type ENUM(
        'income',
        'expense'
    ) NOT NULL,

    icon VARCHAR(50) NULL,
    color VARCHAR(20) NULL,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_financial_categories_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_financial_category_user
        UNIQUE (
            user_id,
            name,
            category_type
        )
);


CREATE TABLE recurring_transactions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,
    account_id BIGINT UNSIGNED NOT NULL,
    category_id BIGINT UNSIGNED NULL,

    transaction_type ENUM(
        'income',
        'expense'
    ) NOT NULL,

    description VARCHAR(200) NOT NULL,

    amount DECIMAL(14,2) NOT NULL,

    frequency ENUM(
        'daily',
        'weekly',
        'monthly',
        'yearly'
    ) NOT NULL,

    start_date DATE NOT NULL,
    end_date DATE NULL,

    next_due_date DATE NOT NULL,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_recurring_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_recurring_account
        FOREIGN KEY (account_id)
        REFERENCES accounts(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_recurring_category
        FOREIGN KEY (category_id)
        REFERENCES financial_categories(id)
        ON DELETE SET NULL,

    CONSTRAINT chk_recurring_amount
        CHECK (
            amount > 0
        ),

    CONSTRAINT chk_recurring_dates
        CHECK (
            end_date IS NULL
            OR end_date >= start_date
        ),

    CONSTRAINT chk_recurring_next_due
        CHECK (
            next_due_date >= start_date
        )
);


CREATE TABLE transactions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,
    account_id BIGINT UNSIGNED NOT NULL,
    category_id BIGINT UNSIGNED NULL,

    recurring_transaction_id BIGINT UNSIGNED NULL,

    transaction_type ENUM(
        'income',
        'expense'
    ) NOT NULL,

    description VARCHAR(200) NOT NULL,

    amount DECIMAL(14,2) NOT NULL,

    transaction_date DATE NOT NULL,

    status ENUM(
        'pending',
        'completed',
        'cancelled'
    ) NOT NULL DEFAULT 'completed',

    notes TEXT NULL,

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_transactions_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_transactions_account
        FOREIGN KEY (account_id)
        REFERENCES accounts(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_transactions_category
        FOREIGN KEY (category_id)
        REFERENCES financial_categories(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_transactions_recurring
        FOREIGN KEY (recurring_transaction_id)
        REFERENCES recurring_transactions(id)
        ON DELETE SET NULL,

    CONSTRAINT chk_transaction_amount
        CHECK (
            amount > 0
        )
);


CREATE TABLE transfers (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    user_id BIGINT UNSIGNED NOT NULL,

    from_account_id BIGINT UNSIGNED NOT NULL,
    to_account_id BIGINT UNSIGNED NOT NULL,

    amount DECIMAL(14,2) NOT NULL,

    transfer_date DATETIME NOT NULL,

    description VARCHAR(200) NULL,

    status ENUM(
        'pending',
        'completed',
        'cancelled'
    ) NOT NULL DEFAULT 'completed',

    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at DATETIME NOT NULL
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT fk_transfers_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_transfers_from_account
        FOREIGN KEY (from_account_id)
        REFERENCES accounts(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_transfers_to_account
        FOREIGN KEY (to_account_id)
        REFERENCES accounts(id)
        ON DELETE RESTRICT,

    CONSTRAINT chk_transfer_amount
        CHECK (
            amount > 0
        ),

    CONSTRAINT chk_transfer_accounts
        CHECK (
            from_account_id <> to_account_id
        )
);




CREATE INDEX idx_categories_user
    ON categories(user_id);

CREATE INDEX idx_activities_user_date
    ON activities(user_id, activity_date);

CREATE INDEX idx_activities_user_started
    ON activities(user_id, started_at);

CREATE INDEX idx_activities_user_category
    ON activities(user_id, category_id);

CREATE INDEX idx_activities_user_status
    ON activities(user_id, status);

CREATE INDEX idx_goals_user_status
    ON goals(user_id, status);

CREATE INDEX idx_goals_user_target_date
    ON goals(user_id, target_date);

CREATE INDEX idx_accounts_user
    ON accounts(user_id);

CREATE INDEX idx_financial_categories_user
    ON financial_categories(user_id);

CREATE INDEX idx_transactions_user_date
    ON transactions(user_id, transaction_date);

CREATE INDEX idx_transactions_account_date
    ON transactions(account_id, transaction_date);

CREATE INDEX idx_transactions_category
    ON transactions(category_id);

CREATE INDEX idx_transactions_recurring
    ON transactions(recurring_transaction_id);

CREATE INDEX idx_transfers_user_date
    ON transfers(user_id, transfer_date);

CREATE INDEX idx_recurring_user_due
    ON recurring_transactions(user_id, next_due_date);

CREATE INDEX idx_recurring_active_due
    ON recurring_transactions(is_active, next_due_date);