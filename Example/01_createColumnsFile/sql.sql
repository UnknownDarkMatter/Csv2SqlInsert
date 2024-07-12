        select t.name as table_name, c.name as column_name, c.user_type_id, c.is_identity
        from sys.tables t
        inner join sys.columns c on t.object_id = c.object_id 