describe('simple tests', () => {
    beforeEach(() => {
        cy.visit('https://woog.life')
    })

    it('temperature contains a dot (`.`)', () => {
        cy.get('#lake-water-temperature').contains(".")
    })
})
