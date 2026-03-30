-- Manual SQL Script to Fix Store Cascade Delete
-- Run this directly in SQL Server Management Studio

-- First, let's check what constraints exist
SELECT name, parent_object_id, referenced_object_id, delete_referential_action_desc
FROM sys.foreign_keys 
WHERE name IN ('FK_Addresses_Stores_StoreId', 'FK_QRCodes_Stores_StoreId');

-- Drop and recreate Address -> Store relationship with CASCADE
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Addresses_Stores_StoreId')
BEGIN
    ALTER TABLE [dbo].[Addresses] DROP CONSTRAINT [FK_Addresses_Stores_StoreId];
    ALTER TABLE [dbo].[Addresses] WITH CHECK ADD CONSTRAINT [FK_Addresses_Stores_StoreId] FOREIGN KEY([StoreId])
    REFERENCES [dbo].[Stores] ([Id])
    ON DELETE CASCADE;
END

-- Drop and recreate QRCode -> Store relationship with CASCADE  
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_QRCodes_Stores_StoreId')
BEGIN
    ALTER TABLE [dbo].[QRCodes] DROP CONSTRAINT [FK_QRCodes_Stores_StoreId];
    ALTER TABLE [dbo].[QRCodes] WITH CHECK ADD CONSTRAINT [FK_QRCodes_Stores_StoreId] FOREIGN KEY([StoreId])
    REFERENCES [dbo].[Stores] ([Id])
    ON DELETE CASCADE;
END

-- Verify the changes
SELECT name, delete_referential_action_desc
FROM sys.foreign_keys 
WHERE name IN ('FK_Addresses_Stores_StoreId', 'FK_QRCodes_Stores_StoreId');
