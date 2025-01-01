
internal static class Files
{
    public static string MetaSQL()
    {
        return @$"

		
-- ****************************
-- ******** Procedures ********
-- ****************************

DECLARE @Result TABLE (ObjectId INT, [Name] VARCHAR(max), [DisplayName] VARCHAR(max), [Schema] VARCHAR(max),Parameters nvarchar(max), ReturnType nvarchar(max), Dependencies nvarchar(max)) ; 

INSERT INTO  @Result (ObjectId, [Name], [DisplayName], [Schema])  
select 
p.object_id, 
p.name AS [Name],
replace (p.name, ' ','_') AS [DisplayName],
s.name as [Schema] 
from sys.procedures p
inner join sys.schemas s on p.schema_id=s.schema_id 
where 
p.is_ms_shipped=0 
--and p.name in  ('x')
order by p.name  


-- Get Parameters
update @Result set Parameters = (
    (
    select   
        right(parameters.name, len(parameters.name) - 1) as [Name],
        replace(right(parameters.name, len(parameters.name) - 1),' ','_') as DisplayName,
        types.name as Type, 
        case when types.name='varchar' and parameters.max_length<0 then 2000 else parameters.max_length end as Length,
        parameters.is_output as IsOut,
        types.is_user_defined as IsUserDefinedType, 
        isnull( types.is_table_type,0 ) as IsUserDefinedTableType
    from sys.parameters parameters 
        left join sys.types types
        on types.system_type_id = parameters.system_type_id and types.user_type_id=parameters.user_type_id		
    where parameters.object_id= res.ObjectId
    order by [Name]
    for JSON Path 
    ) 
)
from @Result res

-- Get ReturnType
update @Result set ReturnType = (
    (
    SELECT distinct
    r.system_type_name as Type, 
    r.name as Name, 
    REPLACE(r.name,' ','_') as DisplayName, 
    r.is_nullable as IsNullable, 
    r.max_length as Length ,
    (case when r.error_type_desc='CONFLICTING_RESULTS' then '1' else 0 end) as IsMultiResult  
    FROM  sys.dm_exec_describe_first_result_set_for_object(res.ObjectId, 0)  AS r 
    order by [Name]
    for JSON Path
    ) 
)
from @Result res

-- Dependencies
UPDATE @Result set Dependencies=(
    SELECT  referenced_entity_name as ChildProcedure 
FROM sys.sql_expression_dependencies AS sed  
INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id  
WHERE referencing_id = res.ObjectId
order by charindex(referenced_entity_name,OBJECT_DEFINITION(res.ObjectId))
for Json PATH
)
from @Result res

SELECT * from @Result 

-- ****************************
-- **** User Defined Types ****
-- ****************************

DECLARE @UDTs TABLE ([Id] int, [Name] VARCHAR(max), [Schema] VARCHAR(max), [DisplayName] VARCHAR(max), Fields nvarchar(max), IsTableType bit) ; 
INSERT INTO  @UDTs ([Id],[Name],[Schema],[DisplayName],Fields, IsTableType)
VALUES(0, 'DUMMY_TYPE','','DUMMY_TYPE', '[]',1)

INSERT INTO  @UDTs ([Id],[Name],[Schema],[DisplayName], IsTableType)  
SELECT   
userDefinedTableTypes.user_type_id as Id,
userDefinedTypes.name AS [Name], 
sch.name AS [Schema],
(ISNULL(sch.name,'') + userDefinedTypes.name) AS [DisplayName],
userDefinedTypes.is_table_type as IsTableType
FROM sys.types AS userDefinedTypes
INNER JOIN sys.schemas AS sch   
	ON sch.schema_id = userDefinedTypes.schema_id   
inner JOIN sys.table_types AS userDefinedTableTypes   
	ON userDefinedTableTypes.user_type_id = userDefinedTypes.user_type_id  
order by [Name]

UPDATE @UDTs set Fields=(
SELECT   
c.name as [Name],
REPLACE(c.name,' ','_') as [DisplayName],
ut.name as [Type] 
FROM sys.types AS t   
inner JOIN sys.table_types AS tt   
	ON tt.user_type_id = t.user_type_id   
left join sys.columns AS c   
	on tt.type_table_object_id=c.object_id  
LEFT JOIN sys.types ut 
 	ON ut.user_type_id = c.user_type_id 
WHERE udts.Id= t.user_type_id 
order by [Name]
for Json Path
)
FROM @UDTs as udts

SELECT * from @UDTs 
	
	
		";
    }
}



