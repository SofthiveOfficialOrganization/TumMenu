-- Fix Store Cascade Delete Constraints
-- This script updates only the Store-related foreign keys to use CASCADE delete
-- without touching the Media table structure

-- Drop and recreate Address -> Store relationship with CASCADE
ALTER TABLE [dbo].[Addresses] DROP CONSTRAINT [FK_Addresses_Stores_StoreId];
ALTER TABLE [dbo].[Addresses] WITH CHECK ADD CONSTRAINT [FK_Addresses_Stores_StoreId] FOREIGN KEY([StoreId])
REFERENCES [dbo].[Stores] ([Id])
ON DELETE CASCADE;

-- Drop and recreate QRCode -> Store relationship with CASCADE  
ALTER TABLE [dbo].[QRCodes] DROP CONSTRAINT [FK_QRCodes_Stores_StoreId];
ALTER TABLE [dbo].[QRCodes] WITH CHECK ADD CONSTRAINT [FK_QRCodes_Stores_StoreId] FOREIGN KEY([StoreId])
REFERENCES [dbo].[Stores] ([Id])
ON DELETE CASCADE;
