import { MapMap, IRange } from '@allors/workspace-system-adapters';

describe('MapMap', () => {
  describe('after construction', () => {
    const mapMap = new MapMap<string, string, IRange<number>>();

    it('should be defined', () => {
      expect(mapMap).toBeDefined();
    });
  });
});
