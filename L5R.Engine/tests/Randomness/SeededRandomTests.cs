using L5R.Engine.Randomness;

namespace L5R.Engine.Tests.Randomness;

public class SeededRandomTests
{
    [Test]
    public void SameSeed_ProducesTheSameSequence()
    {
        var a = new SeededRandom(42);
        var b = new SeededRandom(42);

        var sequenceA = Enumerable.Range(0, 20).Select(_ => a.NextUInt64()).ToArray();
        var sequenceB = Enumerable.Range(0, 20).Select(_ => b.NextUInt64()).ToArray();

        Assert.That(sequenceB, Is.EqualTo(sequenceA));
    }

    [Test]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var a = new SeededRandom(1);
        var b = new SeededRandom(2);

        var sequenceA = Enumerable.Range(0, 20).Select(_ => a.NextUInt64()).ToArray();
        var sequenceB = Enumerable.Range(0, 20).Select(_ => b.NextUInt64()).ToArray();

        Assert.That(sequenceB, Is.Not.EqualTo(sequenceA));
    }

    [Test]
    public void Next_StaysWithinTheExclusiveUpperBound()
    {
        var rng = new SeededRandom(7);

        for (int i = 0; i < 1000; i++)
        {
            var value = rng.Next(6);
            Assert.That(value, Is.InRange(0, 5));
        }
    }

    [Test]
    public void Shuffle_SameSeed_ProducesTheSamePermutation()
    {
        var a = new SeededRandom(99);
        var b = new SeededRandom(99);

        var deckA = Enumerable.Range(1, 40).ToList();
        var deckB = Enumerable.Range(1, 40).ToList();
        a.Shuffle(deckA);
        b.Shuffle(deckB);

        Assert.That(deckB, Is.EqualTo(deckA));
        Assert.That(deckA, Is.EquivalentTo(Enumerable.Range(1, 40)), "shuffle must not lose or duplicate cards");
    }
}
