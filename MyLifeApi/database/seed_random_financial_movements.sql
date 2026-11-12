-- ============================================================
-- BUBU - MOVIMENTACOES FINANCEIRAS DE DEMONSTRACAO
-- Compatibilidade: MySQL 8+
--
-- Antes de executar, altere APENAS as configuracoes abaixo.
-- O script cria entradas e saidas entre 1 de janeiro do ano
-- atual e CURDATE(), portanto nunca cria movimentacoes futuras.
-- ============================================================

SET NAMES utf8mb4;

SET @target_user_email = 'SEU_EMAIL_AQUI';

-- NULL: distribui entre todas as contas ativas do usuario.
-- Exemplo para uma conta especifica: 'Nubank'
SET @target_account_name = NULL;

-- Quantidade permitida: 1 a 500.
SET @movement_count = 80;

-- Identificador do lote. Mantenha o mesmo valor para poder
-- executar novamente sem duplicar as movimentacoes.
SET @batch_key = 'demonstracao-financeira-2026-v1';

DROP PROCEDURE IF EXISTS seed_bubu_financial_movements;

DELIMITER $$

CREATE PROCEDURE seed_bubu_financial_movements()
BEGIN
    DECLARE v_user_id BIGINT UNSIGNED DEFAULT NULL;
    DECLARE v_account_id BIGINT UNSIGNED DEFAULT NULL;
    DECLARE v_account_name VARCHAR(100) DEFAULT NULL;
    DECLARE v_start_date DATE;
    DECLARE v_end_date DATE;
    DECLARE v_transaction_date DATE;
    DECLARE v_created_at DATETIME;
    DECLARE v_transaction_type VARCHAR(10);
    DECLARE v_category_name VARCHAR(80);
    DECLARE v_description VARCHAR(200);
    DECLARE v_notes VARCHAR(255);
    DECLARE v_amount DECIMAL(14,2);
    DECLARE v_index INT DEFAULT 1;
    DECLARE v_selector INT;
    DECLARE v_day_count INT;
    DECLARE v_inserted INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    IF @target_user_email IS NULL OR TRIM(@target_user_email) = ''
       OR @target_user_email = 'SEU_EMAIL_AQUI' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Informe seu e-mail em @target_user_email antes de executar.';
    END IF;

    IF @movement_count IS NULL OR @movement_count < 1 OR @movement_count > 500 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = '@movement_count deve estar entre 1 e 500.';
    END IF;

    IF @batch_key IS NULL OR TRIM(@batch_key) = '' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Informe um identificador em @batch_key.';
    END IF;

    SELECT id
      INTO v_user_id
      FROM users
     WHERE email = LOWER(TRIM(@target_user_email))
       AND status = 'active'
     LIMIT 1;

    IF v_user_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Usuario ativo nao encontrado para o e-mail informado.';
    END IF;

    IF @target_account_name IS NOT NULL AND TRIM(@target_account_name) <> '' THEN
        SELECT id, name
          INTO v_account_id, v_account_name
          FROM accounts
         WHERE user_id = v_user_id
           AND name = TRIM(@target_account_name)
           AND is_active = TRUE
         LIMIT 1;

        IF v_account_id IS NULL THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'A conta informada nao foi encontrada ou esta inativa.';
        END IF;
    ELSEIF NOT EXISTS (
        SELECT 1
          FROM accounts
         WHERE user_id = v_user_id
           AND is_active = TRUE
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'O usuario precisa ter ao menos uma conta ativa.';
    END IF;

    SET v_start_date = MAKEDATE(YEAR(CURDATE()), 1);
    SET v_end_date = CURDATE();
    SET v_day_count = DATEDIFF(v_end_date, v_start_date) + 1;

    START TRANSACTION;

    -- Categorias usadas pelos dados de demonstracao.
    INSERT IGNORE INTO financial_categories
        (user_id, name, category_type, icon, color, is_active)
    VALUES
        (v_user_id, 'Salário',          'income',  'wallet',       '#70c52f', TRUE),
        (v_user_id, 'Freelance',        'income',  'briefcase',    '#2f9e73', TRUE),
        (v_user_id, 'Outras receitas',  'income',  'circle-plus',  '#4d9bd8', TRUE),
        (v_user_id, 'Alimentação',      'expense', 'utensils',     '#ff8a65', TRUE),
        (v_user_id, 'Moradia',          'expense', 'house',        '#d16b86', TRUE),
        (v_user_id, 'Transporte',       'expense', 'car',          '#5c8df6', TRUE),
        (v_user_id, 'Saúde',            'expense', 'heart-pulse',  '#e65f5c', TRUE),
        (v_user_id, 'Lazer',            'expense', 'ticket',       '#9b7af7', TRUE),
        (v_user_id, 'Educação',         'expense', 'book-open',    '#7f67c7', TRUE),
        (v_user_id, 'Assinaturas',      'expense', 'repeat',       '#f5b942', TRUE),
        (v_user_id, 'Compras',          'expense', 'shopping-bag', '#d978b5', TRUE),
        (v_user_id, 'Outras despesas',  'expense', 'receipt',      '#8b95a5', TRUE);

    WHILE v_index <= @movement_count DO
        -- Aproximadamente 25% de entradas e 75% de saidas.
        SET v_selector = MOD(
            CRC32(CONCAT(@batch_key, ':type:', v_index)),
            100
        );
        SET v_transaction_type = IF(v_selector < 25, 'income', 'expense');

        IF v_transaction_type = 'income' THEN
            CASE MOD(CRC32(CONCAT(@batch_key, ':income:', v_index)), 6)
                WHEN 0 THEN
                    SET v_category_name = 'Salário';
                    SET v_description = 'Salário mensal';
                    SET v_amount = 2800 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 220001) / 100;
                WHEN 1 THEN
                    SET v_category_name = 'Freelance';
                    SET v_description = 'Projeto freelance';
                    SET v_amount = 350 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 165001) / 100;
                WHEN 2 THEN
                    SET v_category_name = 'Outras receitas';
                    SET v_description = 'Reembolso recebido';
                    SET v_amount = 40 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 41001) / 100;
                WHEN 3 THEN
                    SET v_category_name = 'Outras receitas';
                    SET v_description = 'Venda de item usado';
                    SET v_amount = 90 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 81001) / 100;
                WHEN 4 THEN
                    SET v_category_name = 'Freelance';
                    SET v_description = 'Servico extra';
                    SET v_amount = 180 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 92001) / 100;
                ELSE
                    SET v_category_name = 'Outras receitas';
                    SET v_description = 'Cashback recebido';
                    SET v_amount = 8 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 14201) / 100;
            END CASE;
        ELSE
            CASE MOD(CRC32(CONCAT(@batch_key, ':expense:', v_index)), 18)
                WHEN 0 THEN SET v_category_name = 'Alimentação'; SET v_description = 'Supermercado';       SET v_amount = 70 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 46001) / 100;
                WHEN 1 THEN SET v_category_name = 'Alimentação'; SET v_description = 'Restaurante';       SET v_amount = 28 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 17201) / 100;
                WHEN 2 THEN SET v_category_name = 'Alimentação'; SET v_description = 'Padaria e café';     SET v_amount = 12 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 6801) / 100;
                WHEN 3 THEN SET v_category_name = 'Moradia';     SET v_description = 'Aluguel';           SET v_amount = 900 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 150001) / 100;
                WHEN 4 THEN SET v_category_name = 'Moradia';     SET v_description = 'Conta de energia';  SET v_amount = 90 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 26001) / 100;
                WHEN 5 THEN SET v_category_name = 'Moradia';     SET v_description = 'Internet';          SET v_amount = 79 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 9101) / 100;
                WHEN 6 THEN SET v_category_name = 'Transporte';  SET v_description = 'Combustível';       SET v_amount = 80 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 27001) / 100;
                WHEN 7 THEN SET v_category_name = 'Transporte';  SET v_description = 'Transporte por app';SET v_amount = 12 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 7601) / 100;
                WHEN 8 THEN SET v_category_name = 'Saúde';       SET v_description = 'Farmácia';          SET v_amount = 25 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 23501) / 100;
                WHEN 9 THEN SET v_category_name = 'Saúde';       SET v_description = 'Consulta médica';   SET v_amount = 120 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 38001) / 100;
                WHEN 10 THEN SET v_category_name = 'Lazer';      SET v_description = 'Cinema';            SET v_amount = 25 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 9501) / 100;
                WHEN 11 THEN SET v_category_name = 'Lazer';      SET v_description = 'Passeio de fim de semana'; SET v_amount = 35 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 26501) / 100;
                WHEN 12 THEN SET v_category_name = 'Educação';   SET v_description = 'Curso online';      SET v_amount = 49 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 35101) / 100;
                WHEN 13 THEN SET v_category_name = 'Educação';   SET v_description = 'Livro';             SET v_amount = 25 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 12501) / 100;
                WHEN 14 THEN SET v_category_name = 'Assinaturas';SET v_description = 'Streaming';         SET v_amount = 19 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 6101) / 100;
                WHEN 15 THEN SET v_category_name = 'Assinaturas';SET v_description = 'Software';          SET v_amount = 25 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 17501) / 100;
                WHEN 16 THEN SET v_category_name = 'Compras';    SET v_description = 'Compra para casa';  SET v_amount = 35 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 36501) / 100;
                ELSE         SET v_category_name = 'Outras despesas'; SET v_description = 'Despesa diversa'; SET v_amount = 15 + MOD(CRC32(CONCAT(@batch_key, ':amount:', v_index)), 18501) / 100;
            END CASE;
        END IF;

        SET v_transaction_date = ADDDATE(
            v_start_date,
            MOD(
                CRC32(CONCAT(@batch_key, ':date:', v_index)),
                v_day_count
            )
        );

        SET v_created_at = TIMESTAMP(
            v_transaction_date,
            MAKETIME(
                8 + MOD(CRC32(CONCAT(@batch_key, ':hour:', v_index)), 13),
                MOD(CRC32(CONCAT(@batch_key, ':minute:', v_index)), 60),
                0
            )
        );
        SET v_created_at = LEAST(v_created_at, NOW());

        -- Se nenhuma conta foi fixada, seleciona uma conta ativa do
        -- usuario de forma deterministica para este item do lote.
        IF @target_account_name IS NULL OR TRIM(@target_account_name) = '' THEN
            SELECT id, name
              INTO v_account_id, v_account_name
              FROM accounts
             WHERE user_id = v_user_id
               AND is_active = TRUE
             ORDER BY CRC32(CONCAT(@batch_key, ':account:', v_index, ':', id))
             LIMIT 1;
        END IF;

        SET v_notes = CONCAT(
            '[BUBU-SEED:', @batch_key, ':', LPAD(v_index, 3, '0'), '] ',
            'Movimentação fictícia para demonstração e testes.'
        );

        INSERT INTO transactions
            (user_id, account_id, category_id, recurring_transaction_id,
             transaction_type, description, amount, transaction_date,
             status, notes, created_at, updated_at)
        SELECT
            v_user_id,
            v_account_id,
            category.id,
            NULL,
            v_transaction_type,
            v_description,
            ROUND(v_amount, 2),
            v_transaction_date,
            'completed',
            v_notes,
            v_created_at,
            v_created_at
        FROM financial_categories category
        WHERE category.user_id = v_user_id
          AND category.name = v_category_name
          AND category.category_type = v_transaction_type
          AND NOT EXISTS (
              SELECT 1
              FROM transactions existing
              WHERE existing.user_id = v_user_id
                AND existing.notes = v_notes
          )
        LIMIT 1;

        SET v_inserted = v_inserted + ROW_COUNT();
        SET v_index = v_index + 1;
    END WHILE;

    COMMIT;

    SELECT
        v_inserted AS movimentacoes_criadas,
        @movement_count - v_inserted AS movimentacoes_ja_existentes,
        v_start_date AS data_inicial,
        v_end_date AS data_final,
        @batch_key AS lote;

    SELECT
        transaction_type AS tipo,
        COUNT(*) AS quantidade,
        ROUND(SUM(amount), 2) AS valor_total
    FROM transactions
    WHERE user_id = v_user_id
      AND notes LIKE CONCAT('[BUBU-SEED:', @batch_key, ':%')
    GROUP BY transaction_type
    ORDER BY transaction_type;
END$$

DELIMITER ;

CALL seed_bubu_financial_movements();
DROP PROCEDURE IF EXISTS seed_bubu_financial_movements;

-- ============================================================
-- PARA APAGAR SOMENTE ESTE LOTE, execute separadamente:
--
-- DELETE FROM transactions
-- WHERE user_id = (SELECT id FROM users WHERE email = LOWER(TRIM(@target_user_email)) LIMIT 1)
--   AND notes LIKE CONCAT('[BUBU-SEED:', @batch_key, ':%');
-- ============================================================
