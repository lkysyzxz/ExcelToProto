# Excel To Proto

## 导表工具功能总览

![image-20250726171056434](Img/image-20250726171056434.png)

## Excel配置说明

&emsp;&emsp;第一行是字段名

&emsp;&emsp;第二行是字段类型

&emsp;&emsp;第三行是注释

&emsp;&emsp;第四行留空

&emsp;&emsp;第五行开始的每一行是内容

&emsp;&emsp;例如：

![image-20250726173135632](Img/image-20250726173135632.png)

## 生成Proto的规则

&emsp;&emsp;一个Excel文件中的所有的Sheet Book都会被翻译成一个Proto文件

&emsp;&emsp;Proto文件的Package定义格式为：**[Excel File Name]Config**

&emsp;&emsp;一个Sheet Book表格生成两个Message，一个用来表示单行数据，一个用来表示多行数据。表示单行数据的Message的名字格式为：**[Sheet Book Name]**，多行数据的Message的名字格式为：**[Sheet Book Name]ConfigArray**，内部仅包含一个数组字段：**[Sheet Book Name]Config**

## Excel类型与Protobuf类型的映射关系

&emsp;&emsp;目前的实现中，Excel表格中的基本类型与Protobuf的类型一一对应。

| Excel Types | Protobuf Types |
| :---------: | :------------: |
|   string    |     string     |
|    int32    |     int32      |
|    int64    |     int64      |
|    float    |     float      |
|   double    |     double     |
|    bool     |      bool      |
|   uint32    |     uint32     |
|   uint64    |     uint64     |
|   sint32    |     sint32     |
|   sint64    |     sint64     |
|   fixed32   |    fixed32     |
|   fixed64   |    fixed64     |
|  sfixed32   |    sfixed32    |
|  sfixed64   |    sfixed64    |

&emsp;&emsp;不支持`bytes`类型。

### 枚举类型

&emsp;&emsp;枚举类型的定义格式为：**Enum_[Name]**。在解析时会将枚举类型定义为`message`的内部类型，枚举值通过扫描表格的每一行来定义。

### 数组类型

&emsp;&emsp;数组类型的定义格式为**list@[Type Name]**，目前只支持基础类型的数组，枚举类型没有尝试和测试过。 

## 配置路径配置规则

&emsp;&emsp;在工程的App.config中进行路径配置。配置规则如下：

| 配置名                  | 含义                                         |
| ----------------------- | -------------------------------------------- |
| ExcelWorkSpace          | Excel表格工作目录                            |
| ProtobufWorkSpace       | Protobuf生成根目录                           |
| SerializedDataWorkSpace | 序列化文件生成的根目录                       |
| ExcelToolDir            | 工具部署目录                                 |
| GameWorkSpace           | 游戏工程配置的存放目录，即序列化文件存放目录 |
| GenerateCodeWorkSpace   | 游戏工程中生成代码的放置目录                 |
| ConfigFilePath          | 序列化文件路径配置存放的目录                 |

&emsp;&emsp;下面是工具部署和目录结构的一个示意，工具和Protoc.exe打包存放在ExcelTool目录中。工程文件配置在上一级目录中

![image-20250726181120378](Img/image-20250726181120378.png)

## Feature List

* 支持数组枚举
* 支持Json格式的文件导出，方便可视化生成结果。