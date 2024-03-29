export type RelationTypeData =
  | [
      roleTag: string,
      associationTag: string,
      roleObjectType: string,
      sizeOrScale?: number,
      precision?: number
    ]
  | [
      roleTag: string,
      associationTag: string,
      roleObjectType: string,
      singularName?: string,
      sizeOrScale?: number,
      precision?: number
    ]
  | [
      roleTag: string,
      associationTag: string,
      roleObjectType: string,
      singularName?: string,
      pluralName?: string,
      sizeOrScale?: number,
      precision?: number
    ];

export type MethodTypeData = [tag: string, name: string];

export type ObjectTypeData = [
  tag: string,
  singularName: string,
  directSupertypes?: string[],
  relationTypes?: RelationTypeData[],
  methodTypes?: MethodTypeData[],
  pluralName?: string
];

export interface MetaData {
  /**
   * Interfaces
   */
  i?: ObjectTypeData[];

  /**
   * Classes
   */
  c?: ObjectTypeData[];

  /**
   * Multiplicity
   */
  m?: string[][];

  /**
   * IsDerived
   */
  d?: string[];

  /**
   * IsRequired
   */
  r?: string[];

  /**
   * MediaType
   */
  t?: { [name: string]: string[] };

  /**
   * Overridden Required
   */
  or?: [string, string[]][];
}
